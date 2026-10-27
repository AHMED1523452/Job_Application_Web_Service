using AutoMapper;
using FluentValidation;
using JobApplication.Application.Features.Jobs.Commands;
using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;
using JobEntity = JobApplication.Domain.Entities.Jobs;

namespace JobApplication.Application.Features.Jobs.Handlers;

public sealed class UpdateJobHandler : IRequestHandler<UpdateJobCommand, GenericResult<JobDetailsResponseDTO>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _user;
    private readonly IMapper _mapper;
    private readonly IBackgroundJobService _backgroundJobs;
    private readonly IApplicationCache _cache;

    public UpdateJobHandler(IUnitOfWork uow, ICurrentUserService user, IMapper mapper, IBackgroundJobService backgroundJobs, IApplicationCache cache)
    {
        _uow = uow;
        _user = user;
        _mapper = mapper;
        _backgroundJobs = backgroundJobs;
        _cache = cache;
    }

    public async Task<GenericResult<JobDetailsResponseDTO>> Handle(UpdateJobCommand request, CancellationToken ct)
    {
        var userId = _user.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<JobDetailsResponseDTO>("Authentication is required.");

        var job = await _uow.Repository<JobEntity>()
            .GetByIdAsyncWithTracking(x => x.Id == request.Id && !x.IsDeleted && !x.IsArchived, ct,
            x => x.HrProfile, x => x.JobCategory, x => x.JobRequirements);

        if (job is null)
            return await Result.FailureAsync<JobDetailsResponseDTO>("Job not found.");
        if (job.HrProfile.UserId != userId)
            return await Result.FailureAsync<JobDetailsResponseDTO>("You can only update your own jobs.");

        var category = await _uow.Repository<JobCategory>().GetByIdAsync(x => x.Id == request.Request.JobCategoryId && !x.IsDeleted && !x.IsArchived, ct);
        if (category is null)
            return await Result.FailureAsync<JobDetailsResponseDTO>("The selected job category was not found.");

        _mapper.Map(request.Request, job);

        job.Title = request.Request.Title.Trim();
        job.Description = request.Request.Description?.Trim();
        job.Address = request.Request.Address?.Trim();
        job.Location = request.Request.Location.Trim();
        job.JobCategory = category;
        job.JobCategoryId = category.Id;
        job.LocationPoint = JobHandlerHelpers.MakePoint(job.Latitude, job.Longitude);
        job.UpdatedAt = DateTime.UtcNow;
        job.UpdatedBy = userId.Value.ToString();
        job.UpdatedByIp = _user.UserIP();

        foreach (var old in job.JobRequirements.ToList())
            await _uow.Repository<JobRequirements>().RemoveAsync(old, ct);

        job.JobRequirements = await JobHandlerHelpers.MapRequirementsAsync(_uow, job.Id, request.Request.Requirements, ct);
        await _uow.Repository<JobEntity>().EditAsync(job, ct);
        await _uow.SaveChangesAsync(ct);
        await _cache.InvalidateAsync("job:", ct);
        await _cache.InvalidateAsync("job-application:", ct);
        if (job.jobStatus == JobStatus.Published && job.ApplicationDeadlineUtc.HasValue)
        {
            var delay = job.ApplicationDeadlineUtc.Value - DateTime.UtcNow;
            await _backgroundJobs.Schedule<IJobDeadlineService>(service => service.CloseIfExpiredAsync(job.Id, ct), delay > TimeSpan.Zero ? delay : TimeSpan.Zero);
        }
        return await Result.SuccessAsync(await JobHandlerHelpers.ToDetailsAsync(_uow, _mapper, job, userId, ct), "Job updated successfully.");
    }
}

public class UpdateJobCommandValidator : AbstractValidator<UpdateJobCommand>
{
    public UpdateJobCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request).NotNull();
    }
}