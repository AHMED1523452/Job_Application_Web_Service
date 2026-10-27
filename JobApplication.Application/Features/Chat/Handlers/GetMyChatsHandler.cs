using FluentValidation;
using JobApplication.Application.Features.Chat.DTOs;
using JobApplication.Application.Features.Chat.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;
using ChatEntity = JobApplication.Domain.Entities.Chat;

namespace JobApplication.Application.Features.Chat.Handlers;

public sealed class GetMyChatsHandler
    : IRequestHandler<GetMyChatsQuery, GenericResult<PaginatedResult<ChatResponseDTO>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    public GetMyChatsHandler(IUnitOfWork unitOfWork,
                             ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<GenericResult<PaginatedResult<ChatResponseDTO>>> Handle(GetMyChatsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue) return await Result.FailureAsync<PaginatedResult<ChatResponseDTO>>("Authentication is required.");

        var (items, totalCount) = await _unitOfWork.Repository<ChatEntity>().GetPagedAsync(
            request.Page,
            request.PageSize,
           predicate: chat => chat.Participants.Any(participant => participant.UserId == userId.Value),
           selector: chat => new ChatResponseDTO
           {
               Id = chat.Id,
               JobApplicationId = chat.JobApplicationId,
               CandidateProfileId = chat.CandidateProfileId,
               HrProfileId = chat.HrProfileId,
               CandidateUserId = chat.CandidateProfile.UserId,
               RecruiterUserId = chat.HrProfile.UserId,
               CreatedAt = chat.CreatedAt,
               LastMessageAt = chat.LastMessageAt
           },
           nameof(ChatEntity.LastMessageAt),
           true,
           cancellationToken: cancellationToken);

        var page = new PaginatedResult<ChatResponseDTO>
        {
            Items = items,
            Message = "Chats loaded.",
            Pagination = new PaginationMetadata
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            }
        };
        return await Result.SuccessAsync(page, "Chats loaded.");
    }
}

public class GetMyChatsQueryValidator : AbstractValidator<GetMyChatsQuery>
{
    public GetMyChatsQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThan(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
