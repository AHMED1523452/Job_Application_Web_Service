using AutoMapper;
using JobApplication.Application.Features.Candidate.Commands;
using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JobApplication.Application.Features.Candidate.Handlers;

public sealed class UploadCandidateCvHandler
    : IRequestHandler<UploadCandidateCvCommand, GenericResult<CandidateCvResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;
    private readonly IMapper _mapper;
    private readonly IApplicationCache _cache;
    private readonly IValidatorService _validator;
    private readonly IFileHashService _fileHashService;
    private readonly IBackgroundJobService _backgroundJobs;
    private readonly ILogger<UploadCandidateCvHandler> _logger;

    public UploadCandidateCvHandler(
                                IUnitOfWork unitOfWork,
                                ICurrentUserService currentUser,
                                IFileStorageService fileStorage,
                                IMapper mapper,
                                IApplicationCache cache,
                                IValidatorService validator,
                                IFileHashService fileHashService,
                                IBackgroundJobService backgroundJobs,
                                ILogger<UploadCandidateCvHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
        _mapper = mapper;
        _cache = cache;
        _validator = validator;
        _fileHashService = fileHashService;
        _backgroundJobs = backgroundJobs;
        _logger = logger;
    }

    public async Task<GenericResult<CandidateCvResponseDTO>> Handle(
        UploadCandidateCvCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<CandidateCvResponseDTO>("Authentication is required.");

        //. Projection: only the profile Id is needed to link the CV — the whole
        //. CandidateProfile entity is never loaded into memory.
        var candidateId = await _unitOfWork.Repository<CandidateProfile>()
            .GetByIdAsNoTrackingAsync(
                profile => profile.UserId == userId.Value &&
                           profile.IsActive &&
                           !profile.IsArchived &&
                           !profile.IsDeleted,
                profile => (Guid?)profile.Id,
                cancellationToken);

        if (candidateId is null)
            return await Result.FailureAsync<CandidateCvResponseDTO>("Create a candidate profile before uploading a CV.");

        var request = command.Request;
        var safeFileName = Path.GetFileName(request.FileName);
        var bucketName = FileCategory.CandidateCV.ToString();

        await using var ms = new MemoryStream();
        await request.FileStream.CopyToAsync(ms, cancellationToken);
        var fileBytes = ms.ToArray();

        var validationResult = _validator.ValidateCandidateCVFile(
            fileBytes,
            safeFileName,
            request.ContentType,
            request.FileSize,
            FileCategory.CandidateCV,
            cancellationToken);

        if (!validationResult.IsValid)
            return await Result.FailureAsync<CandidateCvResponseDTO>(validationResult.Message);

        //. Duplicate validation stays inside the request (fast SHA256 over the uploaded bytes);
        //. the heavy text extraction is deferred to the background job.
        var fileHash = _fileHashService.ComputeHash(fileBytes);

        var isDuplicate = await _fileHashService.IsFileDuplicateAsync(fileHash, cancellationToken);
        if (isDuplicate)
            return await Result.FailureAsync<CandidateCvResponseDTO>(
                "This CV file has already been uploaded. Duplicate files are not allowed.");

        var storageKey = await _fileStorage.UploadAsync(
            new MemoryStream(fileBytes),
            safeFileName,
            bucketName,
            request.ContentType,
            "candidate-cvs",
            cancellationToken);

        var cv = _mapper.Map<CandidateCVs>(request);

        cv.Id = Guid.NewGuid();
        cv.CandidateProfileId = candidateId.Value;
        cv.OriginalFileName = safeFileName;
        cv.StoragePath = storageKey;
        cv.ContentType = request.ContentType;
        cv.FileSize = request.FileSize;
        cv.FileHash = fileHash;
        //. The CV is stored as Processing: its text will be extracted by the background job.
        cv.ProcessingStatus = CvProcesssingStatus.Processing;
        cv.UploadedAtUtc = DateTime.UtcNow;
        cv.ProcessedAtUtc = null;
        cv.Extraction = new CandidateCvExtraction
        {
            Id = Guid.NewGuid(),
            CandidateCvId = cv.Id,
            Status = CvExtractionStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        try
        {
            await _unitOfWork.Repository<CandidateCVs>().AddAsync(cv, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            try
            {
                await _fileStorage.DeleteAsync(storageKey, bucketName, CancellationToken.None);
            }
            catch (Exception cleanupException)
            {
                _logger.LogError(cleanupException,
                    "Could not remove stored CV object {StorageKey} after database save failed.", storageKey);
            }

            _logger.LogError(exception, "Could not save uploaded CV metadata for candidate {CandidateId}.", candidateId.Value);
            throw;
        }

        try
        {
            //. Queue the extraction; the API already answered "accepted" semantics with 202 from the controller.
            await _backgroundJobs.Enqueue<ICvExtractionJob>(
                job => job.ExtractTextAsync(cv.Id, CancellationToken.None));
        }
        catch (Exception exception)
        {
            //. Queueing failed: record the failure on the extraction row and in the error logs
            //. so the state is explicit instead of a CV stuck in Processing forever.
            cv.ProcessingStatus = CvProcesssingStatus.Failed;
            cv.Extraction.Status = CvExtractionStatus.Failed;
            cv.Extraction.ErrorMessage = $"The text extraction job could not be queued: {exception.Message}";
            cv.Extraction.CompletedAtUtc = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(CancellationToken.None); //. updating not 

            _logger.LogError(exception,
                "Could not queue the CV text extraction job for candidate CV {CvId}.", cv.Id);

            return await Result.FailureAsync<CandidateCvResponseDTO>(
                "The CV was uploaded but the text extraction job could not be queued. Please upload the CV again.");
        }

        await _cache.InvalidateAsync("candidate:", cancellationToken);
        return await Result.SuccessAsync(
            _mapper.Map<CandidateCvResponseDTO>(cv),
            "CV uploaded. Text extraction has been queued and is processing in the background.");
    }
}
