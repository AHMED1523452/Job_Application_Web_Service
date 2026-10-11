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
            Body = htmlBody,
            Status = EmailStatus.Pending
        };

        var repository = _unitOfWork.Repository<EmailNotification>();
        await repository.AddAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
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

public sealed class EmailNotificationDeliveryJob : IEmailNotificationDeliveryJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailNotificationDeliveryJob> _logger;

    public EmailNotificationDeliveryJob(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<EmailNotificationDeliveryJob> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 5, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task DeliverAsync(long emailNotificationId, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<EmailNotification>();
        var record = await repository.GetByIdAsyncWithTracking(
            email => email.Id == emailNotificationId,
            cancellationToken);

        if (record is null || record.Status == EmailStatus.Sent)
            return;

        //. increasing the retries count for each calling for this function 
        record.RetryCount++;
        record.Status = EmailStatus.Pending;
        record.FailedAtUtc = null;
        record.ErrorMessage = null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailService.SendEmailAsync(
                record.RecipientEmail,
                record.Subject,
                record.Body,
                cancellationToken);

            record.Status = EmailStatus.Sent;
            record.SentAtUtc = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            //. Editing with tracking in memory first 
            record.Status = EmailStatus.Failed;
            record.FailedAtUtc = DateTime.UtcNow;
            record.ErrorMessage = exception.Message;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogError(exception, "Delivery failed for email notification {EmailNotificationId} (attempt {Attempt}).", record.Id, record.RetryCount);
            throw;
        }
    }
}
