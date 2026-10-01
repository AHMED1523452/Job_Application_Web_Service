namespace JobApplication.Application.Features.Messages.DTOs;

public sealed class MessageResponseDTO
{
    public Guid Id { get; init; }
    public Guid ChatId { get; init; }
    public Guid? SenderId { get; init; }
    public string Content { get; init; } = string.Empty;
    public Guid? MediaFileId { get; init; }
    public bool IsRead { get; init; }
    public DateTime SentAt { get; init; }
}
