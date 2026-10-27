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

public sealed class CreateJobApplicationHandler
    : IRequestHandler<CreateJobApplicationCommand, GenericResult<CreateJobApplicationResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;
    private readonly IEmailNotificationQueue _emailQueue;
    private readonly IApplicationCache _cache;
    private readonly AutoMapper.IMapper _mapper;

    public CreateJobApplicationHandler(
                                       IUnitOfWork unitOfWork,
                                       ICurrentUserService currentUser,
                                       IIdentityService identityService,
                                       IEmailNotificationQueue emailQueue,
                                       IApplicationCache cache,
                                       AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _identityService = identityService;
        _emailQueue = emailQueue;
        _cache = cache;
        _mapper = mapper;
    }

    public async Task<GenericResult<CreateJobApplicationResponseDTO>> Handle(
        CreateJobApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<CreateJobApplicationResponseDTO>("Authentication is required.");

        Guid candidateId = await _unitOfWork.Repository<CandidateProfile>().GetByIdAsyncWithTracking(
            profile => profile.Id == command.candidateProfileId
                && profile.UserId == userId.Value
                && profile.IsActive
                && !profile.IsArchived
                && !profile.IsDeleted,
            selector: op => op.Id,
            cancellationToken);

        if (candidateId == default)
            return await Result.FailureAsync<CreateJobApplicationResponseDTO>(
                "The candidateId profile in the URL does not belong to the authenticated user.");

        var job = await _unitOfWork.Repository<JobEntity>().GetByIdAsyncWithTracking(
            item => item.Id == command.requestDTO.JobId
                && item.IsActive
                && !item.IsArchived
                && !item.IsDeleted,
            cancellationToken);

        //. validation for the deadline timing and current job application status
        //. the job record must be existed in the memory 
        if (job is null || job.jobStatus != JobStatus.Published
            || (job.ApplicationDeadlineUtc.HasValue && job.ApplicationDeadlineUtc <= DateTime.UtcNow))
            return await Result.FailureAsync<CreateJobApplicationResponseDTO>(
                "The job is unavailable or no longer accepting applications.");

        //. validating if the user cv existing in db or not 
        var cvExists = await _unitOfWork.Repository<CandidateCVs>().AnyAsync(
            cv => cv.Id == command.requestDTO.CandidateCvId
                && cv.CandidateProfileId == candidateId,
            cancellationToken);

        if (!cvExists)
            return await Result.FailureAsync<CreateJobApplicationResponseDTO>(
                "The selected CV does not belong to this candidateId profile.");

        var applications = _unitOfWork.Repository<JobApplications>();
        if (await applications.AnyAsync(
                application => application.CandidateProfileId == candidateId
                    && application.JobId == job.Id
                    && application.Status != JobApplicationStatus.Cancelled
                    && !application.IsDeleted,
                cancellationToken))
        {
            return await Result.FailureAsync<CreateJobApplicationResponseDTO>(
                "This candidateId has already applied for this job.");
        }

        var now = DateTime.UtcNow;

        var application = _mapper.Map<JobApplications>(command.requestDTO);
        application.Id = Guid.NewGuid();
        application.CandidateProfileId = candidateId;
        application.Status = JobApplicationStatus.Applied;
        application.AppliedAtUtc = now;
        application.CreatedAt = now;
        application.CreatedBy = userId.Value.ToString();
        application.CreatedByIp = _currentUser.UserIP();
        application.IsActive = true;

        await applications.AddAsync(application, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.InvalidateAsync("job-application:", cancellationToken);
        await _cache.InvalidateAsync($"job:by-id:{job.Id}:", cancellationToken);

        var recruiter = await _unitOfWork.Repository<HrProfileEntity>().GetByIdAsync(
            profile => profile.Id == job.HrProfileId && !profile.IsDeleted && !profile.IsArchived,
            cancellationToken);

        //. a option step 
        if (recruiter is not null)
        {
            var candidateContact = await _identityService.GetUserContactAsync(userId.Value, cancellationToken);
            var recruiterMessage = $"{candidateContact?.FullName ?? "A candidateId"} applied for {job.Title}.";
            await JobApplicationEventNotifier.NotifyAsync(
                _unitOfWork,
                _identityService,
                _emailQueue,
                recruiter.UserId,
                userId.Value,
                NotificationType.JobApplicationCreated,
                "New job application",
                recruiterMessage,
                $"New application for {job.Title}",
                cancellationToken);
        }

        var candidateMessage = $"Your application for {job.Title} was submitted successfully.";
        //. this function that existing inside this function has the Queue async 
        await JobApplicationEventNotifier.NotifyAsync(
            _unitOfWork, _identityService, _emailQueue,
            userId.Value,
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
            CandidateProfileId = candidateId,
            Description = application.CoverLetter,
            Status = application.Status.ToString()
        }, "Job application submitted successfully.");
    }
}

