using JobApplication.Application.Features.HrProfile.Commands;
using JobApplication.Application.Features.HrProfile.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using HrProfileEntity = JobApplication.Domain.Entities.HrProfile;
using MediatR;
using JobApplication.Application.Services;

namespace JobApplication.Application.Features.HrProfile.Handlers;

public sealed class UploadCompanyLogoHandler : IRequestHandler<UploadCompanyLogoCommand, GenericResult<UploadCompanyLogoResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;
    private readonly IValidatorService _validatorService;
    private readonly IIdentityService _identityService;
    private readonly IEmailNotificationQueue _emailQueue;

    public UploadCompanyLogoHandler(
                                    IUnitOfWork unitOfWork,
                                    ICurrentUserService currentUser,
                                    IFileStorageService fileStorage,
                                    IValidatorService validatorService,
                                    IIdentityService identityService,
                                    IEmailNotificationQueue emailQueue)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
        _validatorService = validatorService;
        _identityService = identityService;
        _emailQueue = emailQueue;
    }

    public async Task<GenericResult<UploadCompanyLogoResponseDTO>> Handle(UploadCompanyLogoCommand command, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<UploadCompanyLogoResponseDTO>("Authentication is required.");

        // Read HrProfile using NoTracking (read-only operation)
        var hrProfile = await _unitOfWork.Repository<HrProfileEntity>().GetByIdAsNoTrackingAsync<HrProfileEntity>(
            h => h.UserId == userId.Value,
            null, ct);

        if (hrProfile is null)
            return await Result.FailureAsync<UploadCompanyLogoResponseDTO>("HR profile not found.");

        // If logo already exists, return existing with message (do not overwrite)
        if (!string.IsNullOrWhiteSpace(hrProfile.CompanyLogoPath))
            return await Result.SuccessAsync(new UploadCompanyLogoResponseDTO
            {
                CompanyLogoPath = hrProfile.CompanyLogoPath,
                Message = "The company logo already exists.",
                IsAlreadyExisting = true
            }, "Company logo already exists.");

        // Validate file presence
        if (command.LogoFile is null || command.LogoFile.Length == 0)
            return await Result.FailureAsync<UploadCompanyLogoResponseDTO>("No file provided.");

        // Validate file extension/type/size using IValidatorService
        if (!_validatorService.IsValidFileExtension(
                command.LogoFile.FileName,
                command.LogoFile.ContentType,
                command.LogoFile.Length,
                FileCategory.CompanyLogo))
            return await Result.FailureAsync<UploadCompanyLogoResponseDTO>("Invalid file type, content type, or size.");

        // Upload new file
        var fileName = $"{userId.Value}_{Guid.NewGuid()}_{command.LogoFile.FileName}";
        var fileUrl = await _fileStorage.UploadAsync(
            command.LogoFile.OpenReadStream(),
            fileName,
            FileCategory.CompanyLogo.ToString(),
            command.LogoFile.ContentType,
            "company-logos",
            ct);

        // Get HrProfile with tracking for update
        var hrProfileToUpdate = await _unitOfWork.Repository<HrProfileEntity>().GetByIdAsyncWithTracking(
            h => h.UserId == userId.Value,
            ct);

        if (hrProfileToUpdate is null)
            return await Result.FailureAsync<UploadCompanyLogoResponseDTO>("HR profile not found.");

        hrProfileToUpdate.CompanyLogoPath = fileUrl;
        hrProfileToUpdate.UpdatedAt = DateTime.UtcNow;
        hrProfileToUpdate.UpdatedBy = userId.Value.ToString();
        hrProfileToUpdate.UpdatedByIp = _currentUser.UserIP();

        await _unitOfWork.Repository<HrProfileEntity>().EditAsync(hrProfileToUpdate, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Send email notification via JobApplicationEventNotifier
        await JobApplicationEventNotifier.NotifyAsync(
            _unitOfWork,
            _identityService,
            _emailQueue,
            userId.Value,
            userId.Value,
            NotificationType.JobApplicationUpdated,
            "Company Logo Updated",
            "Your company logo has been updated successfully.",
            "Company Logo Updated",
            ct);

        return await Result.SuccessAsync(new UploadCompanyLogoResponseDTO
        {
            CompanyLogoPath = fileUrl,
            Message = "Company logo uploaded successfully.",
            IsAlreadyExisting = false
        }, "Company logo uploaded successfully.");
    }
}