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
    private readonly ILogger<UploadCandidateCvHandler> _logger;

    public UploadCandidateCvHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        IFileStorageService fileStorage, IMapper mapper, IApplicationCache cache,
        ILogger<UploadCandidateCvHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<GenericResult<CandidateCvResponseDTO>> Handle(
        UploadCandidateCvCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<CandidateCvResponseDTO>("Authentication is required.");

        //. check if the candidate profile exists for the current user
        var candidate = await _unitOfWork.Repository<CandidateProfile>().GetByIdAsync(
            profile => profile.UserId == userId.Value && profile.IsActive
                && !profile.IsArchived && !profile.IsDeleted,
            cancellationToken);
        if (candidate is null)
            return await Result.FailureAsync<CandidateCvResponseDTO>("Create a candidate profile before uploading a CV.");

        var request = command.Request;
        var safeFileName = Path.GetFileName(request.FileName);
        var storageKey = await _fileStorage.UploadAsync(
            request.FileStream,
            safeFileName,
            FileCategory.CandidateCV.ToString(),
            request.ContentType,
            "candidate-cvs",
            cancellationToken);
        var cv = _mapper.Map<CandidateCVs>(request);

        try
        {
            //. save the CV metadata to the database
            cv.Id = Guid.NewGuid();
            cv.CandidateProfileId = candidate.Id;
            cv.CandidateProfile = candidate;
            cv.OriginalFileName = safeFileName;
            cv.StoragePath = storageKey;
            cv.ContentType = request.ContentType;
            cv.FileSize = request.FileSize;
            cv.ProcessingStatus = CvProcesssingStatus.Uploaded;
            cv.UploadedAtUtc = DateTime.UtcNow;

            await _unitOfWork.Repository<CandidateCVs>().AddAsync(cv, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            try
            {
                await _fileStorage.DeleteAsync(
                    storageKey, FileCategory.CandidateCV.ToString(), CancellationToken.None);
            }
            catch (Exception cleanupException)
            {
                _logger.LogError(cleanupException,
                    "Could not remove stored CV object {StorageKey} after the database save failed.", storageKey);
            }

            _logger.LogError(exception, "Could not save uploaded CV metadata for candidate {CandidateId}.", candidate.Id);
            throw;
        }

        await _cache.InvalidateAsync("candidate:", cancellationToken);
        return await Result.SuccessAsync(_mapper.Map<CandidateCvResponseDTO>(cv), "CV uploaded successfully.");
    }
}
