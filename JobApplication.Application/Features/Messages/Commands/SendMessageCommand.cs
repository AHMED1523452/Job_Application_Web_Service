using JobApplication.Application.Features.Messages.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Messages.Commands;

public sealed record SendMessageCommand(Guid ChatId, SendMessageRequestDTO Request)
    : IRequest<GenericResult<MessageResponseDTO>>;
