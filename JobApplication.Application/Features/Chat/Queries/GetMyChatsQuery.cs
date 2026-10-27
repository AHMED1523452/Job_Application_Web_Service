
using JobApplication.Application.Features.Chat.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Chat.Queries
{
    public sealed record GetMyChatsQuery(int Page, int PageSize) : IRequest<GenericResult<PaginatedResult<ChatResponseDTO>>>;
}
