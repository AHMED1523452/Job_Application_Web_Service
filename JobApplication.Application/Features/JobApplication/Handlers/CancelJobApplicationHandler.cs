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

public sealed class CancelJobApplicationHandler
    : IRequestHandler<CancelJobApplicationCommand, GenericResult<CancelJobApplicationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;
    private readonly IEmailNotificationQueue _emailQueue;
    private readonly IApplicationCache _cache;

    public CancelJobApplicationHandler(
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

    public async Task<GenericResult<CancelJobApplicationResponse>> Handle(
        CancelJobApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<CancelJobApplicationResponse>("Authentication is required.");

        var candidate = await _unitOfWork.Repository<CandidateProfile>().GetByIdAsync(
            profile => profile.Id == command.candidateProfileId
                && profile.UserId == userId.Value
                && profile.IsActive
                && !profile.IsArchived
                && !profile.IsDeleted,
            cancellationToken);
        if (candidate is null)
            return await Result.FailureAsync<CancelJobApplicationResponse>(
                "The candidate profile in the URL does not belong to the authenticated user.");

        var applications = _unitOfWork.Repository<JobApplications>();
        var application = await applications.GetByIdAsyncWithTracking(
            item => item.Id == command.appId
                && item.CandidateProfileId == candidate.Id
                && item.Status != JobApplicationStatus.Cancelled
                && item.Status != JobApplicationStatus.Closed
                && !item.IsDeleted,
            cancellationToken);
        if (application is null)
            return await Result.FailureAsync<CancelJobApplicationResponse>("Job application not found.");

        application.Status = JobApplicationStatus.Cancelled;
        application.IsActive = false;
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
                NotificationType.JobApplicationCancelled,
                "Application cancelled",
                $"A candidate cancelled their application for {job.Title}.",
                $"Application cancelled: {job.Title}",
                cancellationToken);
        }

        await JobApplicationEventNotifier.NotifyAsync(
            _unitOfWork, _identityService, _emailQueue,
            candidate.UserId,
            userId.Value,
            NotificationType.JobApplicationCancelled,
            "Application cancelled",
            $"Your application for {job?.Title ?? "the job"} was cancelled.",
            $"Application cancelled: {job?.Title ?? "the job"}",
            cancellationToken);

        return await Result.SuccessAsync(new CancelJobApplicationResponse
        {
            Id = application.Id,
            JobId = application.JobId
        }, "Job application cancelled successfully.");
    }
}
