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

public sealed class CreateJobApplicationHandler
    : IRequestHandler<CreateJobApplicationCommand, GenericResult<CreateJobApplicationResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;
    private readonly IEmailNotificationQueue _emailQueue;

    public CreateJobApplicationHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IIdentityService identityService,
        IEmailNotificationQueue emailQueue)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _identityService = identityService;
        _emailQueue = emailQueue;
    }

    public async Task<GenericResult<CreateJobApplicationResponseDTO>> Handle(
        CreateJobApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<CreateJobApplicationResponseDTO>("Authentication is required.");

        var candidate = await _unitOfWork.Repository<CandidateProfile>().GetByIdAsync(
            profile => profile.Id == command.candidateProfileId
                && profile.UserId == userId.Value
                && profile.IsActive
                && !profile.IsArchived
                && !profile.IsDeleted,
            cancellationToken);

        if (candidate is null)
            return await Result.FailureAsync<CreateJobApplicationResponseDTO>(
                "The candidate profile in the URL does not belong to the authenticated user.");

        var job = await _unitOfWork.Repository<JobEntity>().GetByIdAsync(
            item => item.Id == command.requestDTO.JobId
                && item.IsActive
                && !item.IsArchived
                && !item.IsDeleted,
            cancellationToken);

        if (job is null || job.jobStatus != JobStatus.Published
            || (job.ApplicationDeadlineUtc.HasValue && job.ApplicationDeadlineUtc <= DateTime.UtcNow))
            return await Result.FailureAsync<CreateJobApplicationResponseDTO>(
                "The job is unavailable or no longer accepting applications.");

        var cvExists = await _unitOfWork.Repository<CandidateCVs>().AnyAsync(
            cv => cv.Id == command.requestDTO.CandidateCvId
                && cv.CandidateProfileId == candidate.Id,
            cancellationToken);
        if (!cvExists)
            return await Result.FailureAsync<CreateJobApplicationResponseDTO>(
                "The selected CV does not belong to this candidate profile.");

        var applications = _unitOfWork.Repository<JobApplications>();
        if (await applications.AnyAsync(
                application => application.CandidateProfileId == candidate.Id
                    && application.JobId == job.Id
                    && application.Status != JobApplicationStatus.Cancelled
                    && !application.IsDeleted,
                cancellationToken))
        {
            return await Result.FailureAsync<CreateJobApplicationResponseDTO>(
                "This candidate has already applied for this job.");
        }

        var now = DateTime.UtcNow;
        var application = new JobApplications
        {
            Id = Guid.NewGuid(),
            JobId = job.Id,
            CandidateProfileId = candidate.Id,
            CandidateCvId = command.requestDTO.CandidateCvId,
            CoverLetter = command.requestDTO.Description.Trim(),
            Status = JobApplicationStatus.Applied,
            AppliedAtUtc = now,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = userId.Value.ToString(),
            UpdatedBy = userId.Value.ToString(),
            CreatedByIp = _currentUser.UserIP(),
            IsActive = true
        };

        await applications.AddAsync(application, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var recruiter = await _unitOfWork.Repository<HrProfile>().GetByIdAsync(
            profile => profile.Id == job.HrProfileId && !profile.IsDeleted && !profile.IsArchived,
            cancellationToken);
        if (recruiter is not null)
        {
            var candidateContact = await _identityService.GetUserContactAsync(candidate.UserId, cancellationToken);
            var recruiterMessage = $"{candidateContact?.FullName ?? "A candidate"} applied for {job.Title}.";
            await JobApplicationEventNotifier.NotifyAsync(
                _unitOfWork, _identityService, _emailQueue,
                recruiter.UserId,
                userId.Value,
                NotificationType.JobApplicationCreated,
                "New job application",
                recruiterMessage,
                $"New application for {job.Title}",
                cancellationToken);
        }

        var candidateMessage = $"Your application for {job.Title} was submitted successfully.";
        await JobApplicationEventNotifier.NotifyAsync(
            _unitOfWork, _identityService, _emailQueue,
            candidate.UserId,
            userId.Value,
            NotificationType.JobApplicationCreated,
            "Application submitted",
            candidateMessage,
            $"Application submitted: {job.Title}",
            cancellationToken);

        return await Result.SuccessAsync(new CreateJobApplicationResponseDTO
        {
            Id = application.Id,
            JobId = job.Id,
            JobTitle = job.Title,
            CandidateProfileId = candidate.Id,
            Description = application.CoverLetter,
            Status = application.Status.ToString()
        }, "Job application submitted successfully.");
    }
}
