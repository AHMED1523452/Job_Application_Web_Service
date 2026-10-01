using JobApplication.Application.Features.Chat.DTOs;
using JobApplication.Application.Features.Chat.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;
using ChatEntity = JobApplication.Domain.Entities.Chat;

namespace JobApplication.Application.Features.Chat.Handlers;

public sealed class GetChatByIdHandler : IRequestHandler<GetChatByIdQuery, GenericResult<ChatResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    public GetChatByIdHandler(IUnitOfWork unitOfWork, 
                              ICurrentUserService currentUser)
    { 
        _unitOfWork = unitOfWork; 
        _currentUser = currentUser; 
    }

    public async Task<GenericResult<ChatResponseDTO>> Handle(GetChatByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue) return await Result.FailureAsync<ChatResponseDTO>("Authentication is required.");

        var chat = await _unitOfWork.Repository<ChatEntity>().GetByIdAsNoTrackingAsync(
            entity => entity.Id == request.ChatId && entity.Participants.Any(p => p.UserId == userId.Value),
            entity => new ChatResponseDTO
            {
                Id = entity.Id, JobApplicationId = entity.JobApplicationId,
                CandidateProfileId = entity.CandidateProfileId, HrProfileId = entity.HrProfileId,
                CandidateUserId = entity.CandidateProfile.UserId, RecruiterUserId = entity.HrProfile.UserId,
                CreatedAt = entity.CreatedAt, LastMessageAt = entity.LastMessageAt
            }, cancellationToken);

        return chat is null
            ? await Result.FailureAsync<ChatResponseDTO>("Chat not found or access denied.")
            : await Result.SuccessAsync(chat, "Chat loaded.");
    }
}
