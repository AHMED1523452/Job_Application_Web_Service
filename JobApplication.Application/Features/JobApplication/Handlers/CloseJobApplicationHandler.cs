using FluentValidation;
using JobApplication.Application.Features.JobApplication.Commands;
using JobApplication.Application.Features.JobApplication.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Application.Services;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;
using JobEntity = JobApplication.Domain.Entities.Jobs;
using HrProfileEntity = JobApplication.Domain.Entities.HrProfile;

namespace JobApplication.Application.Features.JobApplication.Handlers;

public sealed class CloseJobApplicationHandler
    : IRequestHandler<CloseJobApplicationCommand, GenericResult<CloseJobApplicationResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;
    private readonly IEmailNotificationQueue _emailQueue;
    private readonly IApplicationCache _cache;

    public CloseJobApplicationHandler(
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
        CloseJobApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<CloseJobApplicationResponseDTO>("Authentication is required.");

        var applications = _unitOfWork.Repository<JobApplications>();
        var application = await applications.GetByIdAsyncWithTracking(
            item => item.Id == command.appId && !item.IsDeleted,
            cancellationToken);
        if (application is null)
            return await Result.FailureAsync<CloseJobApplicationResponseDTO>("Application not found.");
        if (application.Status is JobApplicationStatus.Cancelled or JobApplicationStatus.Closed)
            return await Result.FailureAsync<CloseJobApplicationResponseDTO>("This application is already closed.");

        var job = await _unitOfWork.Repository<JobEntity>().GetByIdAsync(
            item => item.Id == application.JobId && !item.IsDeleted,
            cancellationToken);
        if (job is null)
            return await Result.FailureAsync<CloseJobApplicationResponseDTO>("The related job was not found.");

        var recruiter = await _unitOfWork.Repository<HrProfileEntity>().GetByIdAsync(
            profile => profile.Id == job.HrProfileId
                && profile.UserId == userId.Value
                && !profile.IsDeleted
                && !profile.IsArchived,
            cancellationToken);
        if (recruiter is null)
            return await Result.FailureAsync<CloseJobApplicationResponseDTO>(
                "Only the recruiter who owns this job can close its application.");

        application.Status = JobApplicationStatus.Closed;
        application.IsActive = false;
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
            await JobApplicationEventNotifier.NotifyAsync(
                _unitOfWork, _identityService, _emailQueue,
                candidate.UserId,
                userId.Value,
                NotificationType.JobApplicationClosed,
                "Application closed",
                $"Your application for {job.Title} was closed by the recruiter.",
                $"Application closed: {job.Title}",
                cancellationToken);
        }

        return await Result.SuccessAsync(new CloseJobApplicationResponseDTO
        {
            Id = application.Id,
            Status = application.Status.ToString()
        }, "Application closed successfully.");
    }
}

public class CloseJobApplicationValidator : AbstractValidator<CloseJobApplicationCommand>
{
    public CloseJobApplicationValidator() => RuleFor(command => command.appId).NotEmpty();
}
