using JobApplication.Domain.Enums;

namespace JobApplication.Application.Features.Notifications.DTOs;

public sealed class NotificationResponseDTO
{
    public long Id { get; init; }
    public NotificationType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReadAtUtc { get; init; }
}
