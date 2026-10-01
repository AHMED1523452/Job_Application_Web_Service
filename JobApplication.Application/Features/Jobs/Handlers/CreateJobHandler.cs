using AutoMapper;
using JobApplication.Application.Features.Jobs.Commands;
using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;

using JobEntity = JobApplication.Domain.Entities.Jobs;
namespace JobApplication.Application.Features.Jobs.Handlers;

public sealed class CreateJobHandler : IRequestHandler<CreateJobCommand, GenericResult<JobDetailsResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IBackgroundJobService _backgroundJobs;

    public CreateJobHandler(IUnitOfWork unitOfWork,
                            ICurrentUserService currentUser,
                            IMapper mapper, 
                            IBackgroundJobService backgroundJobs)
    {   
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _backgroundJobs = backgroundJobs; }

    public async Task<GenericResult<JobDetailsResponseDTO>> Handle(CreateJobCommand command, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        //. Another Validating if the user is authenticated or not
        if (!userId.HasValue)
            return await Result.FailureAsync<JobDetailsResponseDTO>("Authentication is required.");

        //. Another Validating if the user is authenticated and Hr role or not 
        var recruiter = await _unitOfWork.Repository<HrProfile>().GetByIdAsync(x => x.UserId == userId.Value, ct);
        if (recruiter is null) return await Result.FailureAsync<JobDetailsResponseDTO>("Create a recruiter profile before posting a job.");

        var category = await _unitOfWork.Repository<JobCategory>().GetByIdAsync(
            x => x.Id == command.Request.JobCategoryId && !x.IsDeleted && !x.IsArchived, ct);
        if (category is null) return await Result.FailureAsync<JobDetailsResponseDTO>("The selected job category was not found.");

        var job = _mapper.Map<JobEntity>(command.Request);
        job.Id = Guid.NewGuid();
        job.Title = command.Request.Title.Trim();
        job.Description = command.Request.Description?.Trim();
        job.Address = command.Request.Address?.Trim();
        job.Location = command.Request.Location.Trim();
        job.HrProfileId = recruiter.Id;
        job.JobCategory = category;
        job.HrProfile = recruiter;
        job.jobStatus = command.Request.InitialStatus;
        job.IsActive = job.jobStatus == JobStatus.Published;
        job.LocationPoint = JobHandlerHelpers.MakePoint(job.Latitude, job.Longitude);
        job.JobRequirements = await JobHandlerHelpers.MapRequirementsAsync(_unitOfWork, job.Id, command.Request.Requirements, ct);
        job.CreatedAt = DateTime.UtcNow;
        job.CreatedBy = userId.Value.ToString();
        job.CreatedByIp = _currentUser.UserIP();

        await _unitOfWork.Repository<JobEntity>().AddAsync(job, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await ScheduleDeadlineCloseAsync(job);
        return await Result.SuccessAsync(await JobHandlerHelpers.ToDetailsAsync(_unitOfWork, _mapper, job, userId, ct), "Job created successfully.");
    }

    private Task ScheduleDeadlineCloseAsync(JobEntity job)
    {
        if (job.jobStatus != JobStatus.Published || !job.ApplicationDeadlineUtc.HasValue) return Task.CompletedTask;
        var delay = job.ApplicationDeadlineUtc.Value - DateTime.UtcNow;
        return _backgroundJobs.Schedule<IJobDeadlineService>(service => service.CloseIfExpiredAsync(job.Id), delay > TimeSpan.Zero ? delay : TimeSpan.Zero);
    }
}
