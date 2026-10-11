using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;

namespace JobApplication.Infrastructure.Services;

public sealed class JobDeadlineService : IJobDeadlineService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IApplicationCache _cache;
    public JobDeadlineService(IUnitOfWork unitOfWork, IApplicationCache cache)
    {
        this.unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task CloseIfExpiredAsync(Guid jobId)
    {
        ///. is the job existed ? 
        //. is the job published ?
        //. is the job has a job application deadline ?
        //. is the job application deadline has passed ?
        var job = await unitOfWork.Repository<Jobs>().GetByIdAsync(x => x.Id == jobId, CancellationToken.None);
        if (job is null || job.jobStatus != JobStatus.Published || !job.ApplicationDeadlineUtc.HasValue
            || job.ApplicationDeadlineUtc.Value > DateTime.UtcNow)
            return;

        job.jobStatus = JobStatus.Closed;
        job.IsActive = false;
        job.UpdatedAt = DateTime.UtcNow;
        job.UpdatedBy = "system:job-deadline";

        await unitOfWork.SaveChangesAsync();
        await _cache.InvalidateAsync("job:");
        await _cache.InvalidateAsync("job-application:");
    }
}
