using JobApplication.Application.Features.Notifications.DTOs;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Mapping;

public static class NotificationMapper
{
    //. Explisit Mapping 
    public static NotificationResponseDTO ToResponse(Notification notification) => new()
    {
        Id = notification.Id,
        Type = notification.Type,
        Title = notification.Title,
        Message = notification.Message,
        IsRead = notification.IsRead,
        CreatedAt = notification.CreatedAt,
        ReadAtUtc = notification.ReadAtUtc
    };
}
