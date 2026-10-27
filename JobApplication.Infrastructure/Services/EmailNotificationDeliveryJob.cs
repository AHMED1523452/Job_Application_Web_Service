using Hangfire;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace JobApplication.Infrastructure.Services
{
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

            //. validating if the record is null or had been sent already
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

}
