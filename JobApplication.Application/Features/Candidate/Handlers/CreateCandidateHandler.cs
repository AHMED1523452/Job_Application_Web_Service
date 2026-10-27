using AutoMapper;
using FluentValidation;
using JobApplication.Application.Features.Candidate.Commands;
using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JobApplication.Application.Features.Candidate.Handlers;

public sealed class CreateCandidateHandler :
    IRequestHandler<CreateCandidateCommand, GenericResult<CandidateResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IApplicationCache _cache;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<CreateCandidateHandler> _logger;

    public CreateCandidateHandler(IUnitOfWork unitOfWork,
                                  ICurrentUserService currentUser,
                                  IMapper mapper,
                                  IApplicationCache cache,
                                  IFileStorageService fileStorage,
                                  ILogger<CreateCandidateHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _cache = cache;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task<GenericResult<CandidateResponseDTO>> Handle(CreateCandidateCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<CandidateResponseDTO>("Authentication is required.");

        var candidates = _unitOfWork.Repository<Domain.Entities.CandidateProfile>();

        if (await candidates.AnyAsync(candidate => candidate.UserId == userId.Value, cancellationToken))
            return await Result.FailureAsync<CandidateResponseDTO>("A candidate profile already exists for this account.");

        var candidate = _mapper.Map<Domain.Entities.CandidateProfile>(command.Request);
        candidate.Skills = _mapper.Map<List<CandidateSkill>>(command.Request.Skills);
        candidate.experiences = _mapper.Map<List<CandidateExperience>>(command.Request.Experiences);
        candidate.educations = _mapper.Map<List<CandidateEducation>>(command.Request.Educations);
        candidate.certifications = _mapper.Map<List<CandidateCertification>>(command.Request.Certifications);

        candidate.Preference = command.Request.Preference is null
            ? null
            : _mapper.Map<CandidatePreference>(command.Request.Preference);

        candidate.Id = Guid.NewGuid();
        candidate.UserId = userId.Value;
        candidate.IsActive = true;
        candidate.CreatedAt = DateTime.UtcNow;
        candidate.UpdatedAt = candidate.CreatedAt;
        candidate.CreatedBy = userId.Value.ToString();
        candidate.UpdatedBy = candidate.CreatedBy;
        candidate.CreatedByIp = _currentUser.UserIP();

        CandidateProfileInitializer.InitializeChildren(candidate, candidate.CreatedBy);

        string? uploadedImagePath = null;
        if (command.ProfileImage is not null)
        {
            var image = command.ProfileImage;
            var safeFileName = Path.GetFileName(image.FileName);

            uploadedImagePath = await _fileStorage.UploadAsync(
                image.FileStream,
                safeFileName,
                FileCategory.ProfileImage.ToString(),
                image.ContentType,
                "candidate-profile-images",
                cancellationToken);

            var mediaFile = new MediaFile
            {
                MediaFileID = Guid.NewGuid(),
                FileName = safeFileName,
                StoragePath = uploadedImagePath,
                ContentType = image.ContentType,
                FileExtension = Path.GetExtension(safeFileName).ToLowerInvariant(),
                FileSize = image.FileSize,
                MediaType = MediaType.ProfileImage,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = candidate.CreatedBy,
                CreatedByIp = candidate.CreatedByIp
            };

            candidate.ProfileImageId = mediaFile.MediaFileID;
            candidate.ProfileImage = mediaFile;
            await _unitOfWork.Repository<MediaFile>().AddAsync(mediaFile, cancellationToken);
        }

        try
        {
            await candidates.AddAsync(candidate, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            if (uploadedImagePath is not null)
            {
                try
                {
                    await _fileStorage.DeleteAsync(
                        uploadedImagePath, FileCategory.ProfileImage.ToString(), CancellationToken.None);
                }
                catch (Exception cleanupException)
                {
                    _logger.LogError(cleanupException,
                        "Could not remove uploaded profile image {StoragePath} after candidate creation failed.",
                        uploadedImagePath);
                }
            }

            _logger.LogError(exception, "Could not save candidate profile for user {UserId}.", userId.Value);
            throw;
        }

        await _cache.InvalidateAsync("candidate:", cancellationToken);
        await _cache.InvalidateAsync("job-application:", cancellationToken);

        return await Result.SuccessAsync(
            await CandidateResponseBuilder.BuildAsync(candidate, _unitOfWork, _mapper, cancellationToken),
            "Candidate profile created successfully.");
    }
}