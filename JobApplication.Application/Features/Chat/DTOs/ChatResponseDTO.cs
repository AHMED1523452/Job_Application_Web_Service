namespace JobApplication.Application.Features.Chat.DTOs;

public class ChatResponseDTO
{
    public Guid Id { get; init; }
    public Guid? JobApplicationId { get; init; }
    public Guid CandidateProfileId { get; init; }
    public Guid HrProfileId { get; init; }
    public Guid CandidateUserId { get; init; }
    public Guid RecruiterUserId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastMessageAt { get; init; }
}
