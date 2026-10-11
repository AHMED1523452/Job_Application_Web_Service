using JobApplication.Application.Features.JobApplication.Commands;
using JobApplication.Application.Features.JobApplication.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Application.Services;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;
using JobEntity = JobApplication.Domain.Entities.Jobs;

namespace JobApplication.Application.Features.JobApplication.Handlers;

public sealed class UpdateJobApplicationHandler
    : IRequestHandler<UpdateJobApplicationCommand, GenericResult<UpdateApplicationResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;
    private readonly IEmailNotificationQueue _emailQueue;
    private readonly IApplicationCache _cache;

    public UpdateJobApplicationHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IIdentityService identityService,
        IEmailNotificationQueue emailQueue,
        IApplicationCache cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _identityService = identityService;
        _emailQueue = emailQueue;
        _cache = cache;
    }

    public async Task<GenericResult<UpdateApplicationResponseDTO>> Handle(
        UpdateJobApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<UpdateApplicationResponseDTO>("Authentication is required.");

        var candidateExists = await _unitOfWork.Repository<CandidateProfile>().AnyAsync(
            profile => profile.Id == command.candidateProfileId
                && profile.UserId == userId.Value
                && profile.IsActive
                && !profile.IsArchived
                && !profile.IsDeleted,
            cancellationToken);
        if (!candidateExists)
            return await Result.FailureAsync<UpdateApplicationResponseDTO>(
                "The candidate profile in the URL does not belong to the authenticated user.");

        var applications = _unitOfWork.Repository<JobApplications>();
        var application = await applications.GetByIdAsyncWithTracking(
            item => item.Id == command.id
                && item.CandidateProfileId == command.candidateProfileId
                && item.Status != JobApplicationStatus.Cancelled
                && item.Status != JobApplicationStatus.Closed
                && !item.IsDeleted,
            cancellationToken);
        if (application is null)
            return await Result.FailureAsync<UpdateApplicationResponseDTO>("Job application not found.");

        application.CoverLetter = command.requestDTO.Description.Trim();
        application.UpdatedAt = DateTime.UtcNow;
        application.UpdatedBy = userId.Value.ToString();
        application.UpdatedByIp = _currentUser.UserIP();
        await applications.EditAsync(application, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.InvalidateAsync("job-application:", cancellationToken);

        var job = await _unitOfWork.Repository<JobEntity>().GetByIdAsync(
            item => item.Id == application.JobId,
            cancellationToken);
        var recruiter = job is null
            ? null
            : await _unitOfWork.Repository<HrProfile>().GetByIdAsync(
                profile => profile.Id == job.HrProfileId && !profile.IsDeleted && !profile.IsArchived,
                cancellationToken);

        if (job is not null && recruiter is not null)
        {
            await JobApplicationEventNotifier.NotifyAsync(
                _unitOfWork, _identityService, _emailQueue,
                recruiter.UserId,
                userId.Value,
                NotificationType.JobApplicationUpdated,
                "Application updated",
                $"A candidate updated their application for {job.Title}.",
                $"Application updated: {job.Title}",
                cancellationToken);
        }

        return await Result.SuccessAsync(new UpdateApplicationResponseDTO
        {
            Id = application.Id,
            JobId = application.JobId,
            Description = application.CoverLetter ?? string.Empty
        }, "Job application updated successfully.");
    }
}
