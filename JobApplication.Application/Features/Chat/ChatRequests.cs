using JobApplication.Application.Features.Chat.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Chat.Commands
{
    public sealed record CreateChatCommand(CreateChatRequestDTO Request) : IRequest<GenericResult<CreateChatResponseDTO>>;
}

//. i thought thaat these things don't need to specific class for each query , are you understanding what i mean !!?
namespace JobApplication.Application.Features.Chat.Queries
{
    public sealed record GetMyChatsQuery(int Page = 1, int PageSize = 20) : IRequest<GenericResult<PaginatedResult<ChatResponseDTO>>>;
    public sealed record GetChatByIdQuery(Guid ChatId) : IRequest<GenericResult<ChatResponseDTO>>;
}
