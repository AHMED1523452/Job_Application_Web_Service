
using JobApplication.Application.Features.Chat.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Chat.Commands
{
    public sealed record CreateChatCommand(CreateChatRequestDTO requestDTO) : IRequest<GenericResult<CreateChatResponseDTO>>;
}
