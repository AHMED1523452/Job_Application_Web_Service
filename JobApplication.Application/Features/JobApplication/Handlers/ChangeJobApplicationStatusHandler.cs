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

public sealed class ChangeJobApplicationStatusHandler
    : IRequestHandler<ChangeJobApplicationStatusCommand, GenericResult<CloseJobApplicationResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;
    private readonly IEmailNotificationQueue _emailQueue;
    private readonly IApplicationCache _cache;

    public ChangeJobApplicationStatusHandler(
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

    public async Task<GenericResult<CloseJobApplicationResponseDTO>> Handle(
        ChangeJobApplicationStatusCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<CloseJobApplicationResponseDTO>("Authentication is required.");

        var applications = _unitOfWork.Repository<JobApplications>();
        var application = await applications.GetByIdAsyncWithTracking(
            item => item.Id == command.ApplicationId && !item.IsDeleted,
            cancellationToken);
        if (application is null)
            return await Result.FailureAsync<CloseJobApplicationResponseDTO>("Application not found.");

        var job = await _unitOfWork.Repository<JobEntity>().GetByIdAsync(
            item => item.Id == application.JobId && !item.IsDeleted,
            cancellationToken);
        if (job is null)
            return await Result.FailureAsync<CloseJobApplicationResponseDTO>("The related job was not found.");

        var recruiter = await _unitOfWork.Repository<HrProfile>().GetByIdAsync(
            profile => profile.Id == job.HrProfileId
                && profile.UserId == userId.Value
                && !profile.IsDeleted
                && !profile.IsArchived,
            cancellationToken);
        if (recruiter is null)
            return await Result.FailureAsync<CloseJobApplicationResponseDTO>(
                "Only the recruiter who owns this job can change its application status.");

        if (!CanTransition(application.Status, command.Request.Status))
            return await Result.FailureAsync<CloseJobApplicationResponseDTO>(
                $"Cannot change application status from {application.Status} to {command.Request.Status}.");

        application.Status = command.Request.Status;
        application.RejectionReason = command.Request.Status == JobApplicationStatus.Rejected
            ? command.Request.RejectionReason?.Trim()
            : null;
        application.ReviewedAtUtc = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;
        application.UpdatedBy = userId.Value.ToString();
        application.UpdatedByIp = _currentUser.UserIP();
        await applications.EditAsync(application, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.InvalidateAsync("job-application:", cancellationToken);

        var candidate = await _unitOfWork.Repository<CandidateProfile>().GetByIdAsync(
            profile => profile.Id == application.CandidateProfileId,
            cancellationToken);
        if (candidate is not null)
        {
            var message = command.Request.Status == JobApplicationStatus.Rejected
                ? $"Your application for {job.Title} was rejected. {application.RejectionReason}"
                : $"Your application for {job.Title} is now {application.Status}.";
            await JobApplicationEventNotifier.NotifyAsync(
                _unitOfWork, _identityService, _emailQueue,
                candidate.UserId,
                userId.Value,
                NotificationType.JobApplicationStatusChanged,
                "Application status updated",
                message,
                $"Application update: {job.Title}",
                cancellationToken);
        }

        return await Result.SuccessAsync(new CloseJobApplicationResponseDTO
        {
            Id = application.Id,
            Status = application.Status.ToString()
        }, "Application status updated successfully.");
    }

    private static bool CanTransition(JobApplicationStatus current, JobApplicationStatus next) => current switch
    {
        JobApplicationStatus.Applied => next is JobApplicationStatus.UnderReview
            or JobApplicationStatus.InterviewScheduled
            or JobApplicationStatus.Accepted
            or JobApplicationStatus.Rejected,
        JobApplicationStatus.UnderReview => next is JobApplicationStatus.InterviewScheduled
            or JobApplicationStatus.Accepted
            or JobApplicationStatus.Rejected,
        JobApplicationStatus.InterviewScheduled => next is JobApplicationStatus.Accepted
            or JobApplicationStatus.Rejected,
        _ => false
    };
}
