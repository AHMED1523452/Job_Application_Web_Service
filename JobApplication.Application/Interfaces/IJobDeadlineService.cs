using JobApplication.Domain.Entities;

namespace JobApplication.Application.Interfaces;

public interface IJobDeadlineService
{
    Task CloseIfExpiredAsync(Guid jobId, CancellationToken cancellationToken);
    Task ScheduleDeadlineCloseAsync(Jobs job, CancellationToken cancellationToken);
}
