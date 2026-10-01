namespace JobApplication.Application.Features.Messages.DTOs;

public sealed class SendMessageRequestDTO
{
    public string? Content { get; init; }
    public Guid? MediaFileId { get; init; }
}
