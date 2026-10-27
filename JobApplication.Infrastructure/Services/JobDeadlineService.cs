using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;

namespace JobApplication.Infrastructure.Services;

public sealed class JobDeadlineService : IJobDeadlineService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IApplicationCache _cache;
    private readonly IBackgroundJobService jobService;

    public JobDeadlineService(IUnitOfWork unitOfWork, 
                              IApplicationCache cache, 
                              IBackgroundJobService jobService)
    {
        this.unitOfWork = unitOfWork;
        _cache = cache;
        this.jobService = jobService;
    }

    public async Task CloseIfExpiredAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        ///. is the job existed ? 
        //. is the job published ?
        //. is the job has a job application deadline ?
        //. is the job application deadline has passed ?
        var job = await unitOfWork.Repository<Jobs>().GetByIdAsyncWithTracking(x => x.Id == jobId && 
                                                                                   !x.IsDeleted && 
                                                                                   !x.IsArchived && 
                                                                                   x.IsActive, CancellationToken.None);
        if (job is null || job.jobStatus != JobStatus.Published || !job.ApplicationDeadlineUtc.HasValue
            || job.ApplicationDeadlineUtc.Value > DateTime.UtcNow)
            return;

        job.jobStatus = JobStatus.Closed;
        job.IsActive = false;
        job.UpdatedAt = DateTime.UtcNow;
        job.UpdatedBy = "system:job-deadline";

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.InvalidateAsync("job:");
        await _cache.InvalidateAsync("job-application:");
    }

    public Task ScheduleDeadlineCloseAsync(Jobs job, CancellationToken cancellationToken = default)
    {
        if (job.jobStatus != JobStatus.Published || !job.ApplicationDeadlineUtc.HasValue)
            return Task.CompletedTask;

        //. making a delay for sending t
        var delay = job.ApplicationDeadlineUtc.Value - DateTime.UtcNow;
        return jobService.Schedule<IJobDeadlineService>(service => service.CloseIfExpiredAsync(job.Id, cancellationToken: cancellationToken), delay > TimeSpan.Zero ? delay : TimeSpan.Zero);
    }
}
