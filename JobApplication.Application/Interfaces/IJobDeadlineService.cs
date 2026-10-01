namespace JobApplication.Application.Interfaces;

public interface IJobDeadlineService
{
    Task CloseIfExpiredAsync(Guid jobId);
}
