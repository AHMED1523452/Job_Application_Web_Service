using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;
using JobApplication.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobApplication.Infrastructure.Services;

public sealed class JobDeadlineService : IJobDeadlineService
{
    private readonly AppDbContext _db;
    public JobDeadlineService(AppDbContext db) => _db = db;

    public async Task CloseIfExpiredAsync(Guid jobId)
    {
        var job = await _db.Jobs.SingleOrDefaultAsync(x => x.Id == jobId);
        if (job is null || job.jobStatus != JobStatus.Published || !job.ApplicationDeadlineUtc.HasValue
            || job.ApplicationDeadlineUtc.Value > DateTime.UtcNow)
            return;

        job.jobStatus = JobStatus.Closed;
        job.IsActive = false;
        job.UpdatedAt = DateTime.UtcNow;
        job.UpdatedBy = "system:job-deadline";
        await _db.SaveChangesAsync();
    }
}
