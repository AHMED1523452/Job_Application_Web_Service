using FluentValidation;
using JobApplication.Application.Features.Messages.DTOs;
using JobApplication.Application.Features.Messages.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Messages.Handlers;

public sealed class GetChatMessagesHandler : IRequestHandler<GetChatMessagesQuery, GenericResult<PaginatedResult<MessageResponseDTO>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetChatMessagesHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<GenericResult<PaginatedResult<MessageResponseDTO>>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<PaginatedResult<MessageResponseDTO>>("Authentication is required.");

        if (!await _unitOfWork.Repository<ChatParticipant>().AnyAsync(
                participant => participant.ChatId == request.ChatId && participant.UserId == userId.Value,
                cancellationToken))
            return await Result.FailureAsync<PaginatedResult<MessageResponseDTO>>("Chat not found or access denied.");

        var (items, totalCount) = await _unitOfWork.Repository<Message>().GetPagedAsync(
            request.Page,
            request.PageSize,
            message => message.ChatId == request.ChatId,
            message => new MessageResponseDTO
            {
                Id = message.Id,
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                Content = message.Content,
                MediaFileId = message.MediaFileId,
                IsRead = message.IsRead,
                SentAt = message.SentAt
            },
            nameof(Message.SentAt),
            orderByDescending: true,
            cancellationToken: cancellationToken);

        var page = new PaginatedResult<MessageResponseDTO>
        {
            Items = items,
            Message = "Messages loaded.",
            Pagination = new PaginationMetadata
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            }
        };
        return await Result.SuccessAsync(page, "Messages loaded.");
    }
}

public class GetChatMessagesQueryValidator : AbstractValidator<GetChatMessagesQuery>
{
    public GetChatMessagesQueryValidator()
    {
        RuleFor(query => query.ChatId).NotEmpty();
        RuleFor(query => query.Page).GreaterThan(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
