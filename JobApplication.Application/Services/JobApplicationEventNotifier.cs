using System.Net;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Services;

/// <summary>Creates the in-app event notification and queues its matching email.</summary>
public static class JobApplicationEventNotifier
{
    public static async Task NotifyAsync(
                                          IUnitOfWork unitOfWork,
                                          IIdentityService identityService,
                                          IEmailNotificationQueue emailQueue,
                                          Guid recipientUserId,
                                          Guid actorUserId,
                                          NotificationType type,
                                          string title,
                                          string message,
                                          string emailSubject,
                                          CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            UserId = recipientUserId,
            Type = type,
            Title = title,
            Message = message,
            IsRead = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = actorUserId.ToString(),
            UpdatedBy = actorUserId.ToString()
        };

        var repository = unitOfWork.Repository<Notification>();
        await repository.AddAsync(notification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var recipient = await identityService.GetUserContactAsync(recipientUserId, cancellationToken);
        if (recipient is null)
            return;

        await emailQueue.QueueAsync(
            recipient.UserId,
            recipient.Email,
            emailSubject,
            $"<p>{WebUtility.HtmlEncode(message)}</p>",
            notification.Id,
            cancellationToken);
    }
}
