namespace JobApplication.Application.Interfaces;

public interface IEmailNotificationQueue
{
    Task QueueAsync(
        Guid userId,
        string recipientEmail,
        string subject,
        string htmlBody,
        long? notificationId = null,
        CancellationToken cancellationToken = default);
}

public interface IEmailNotificationDeliveryJob
{
    Task DeliverAsync(long emailNotificationId, CancellationToken cancellationToken);
}
