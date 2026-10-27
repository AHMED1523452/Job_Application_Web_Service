using Hangfire;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace JobApplication.Infrastructure.Services;

public sealed class EmailNotificationQueue : IEmailNotificationQueue
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobService _backgroundJobs;
    private readonly ILogger<EmailNotificationQueue> _logger;

    public EmailNotificationQueue(
                                    IUnitOfWork unitOfWork,
                                    IBackgroundJobService backgroundJobs,
                                    ILogger<EmailNotificationQueue> logger)
    {
        _unitOfWork = unitOfWork;
        _backgroundJobs = backgroundJobs;
        _logger = logger;   
    }

    public async Task QueueAsync(
        Guid userId,
        string recipientEmail,
        string subject,
        string htmlBody,
        long? notificationId = null,
        CancellationToken cancellationToken = default)
    {
        var brandedSubject = subject.StartsWith("HireSense", StringComparison.OrdinalIgnoreCase)
            ? subject
            : $"HireSense | {subject}";

        var record = new EmailNotification
        {
            UserId = userId,
            NotificationId = notificationId,
            RecipientEmail = recipientEmail,
            Subject = brandedSubject,
            Body = subject,
            Status = EmailStatus.Pending
        };

        var repository = _unitOfWork.Repository<EmailNotification>();
        await repository.AddAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            //. don't mess this line (review this content) 
            await _backgroundJobs.Enqueue<EmailNotificationDeliveryJob>(
                job => job.DeliverAsync(record.Id, CancellationToken.None)); //. AS Enqueue in hangfire
        }
        catch (Exception exception)
        {
            record.Status = EmailStatus.Failed;
            record.FailedAtUtc = DateTime.UtcNow;
            record.ErrorMessage = $"Email could not be queued: {exception.Message}";

            await repository.EditAsync(record, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogError(exception, "Unable to queue email notification {EmailNotificationId}.", record.Id);
        }
    }
}