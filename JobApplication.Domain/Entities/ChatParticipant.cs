namespace JobApplication.Domain.Entities;

/// <summary>Associates an application user with a chat and stores that participant's chat read position.</summary>
public sealed class ChatParticipant
{
    public Guid ChatId { get; set; }
    public Chat Chat { get; set; } = null!;

    public Guid UserId { get; set; }

    public DateTime JoinedAtUtc { get; set; }
    public DateTime? LastReadAtUtc { get; set; }
}
