using JobApplication.Application.Features.Messages.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Messages.Queries;

public sealed record GetChatMessagesQuery(Guid ChatId, int Page = 1, int PageSize = 30)
    : IRequest<GenericResult<PaginatedResult<MessageResponseDTO>>>;
