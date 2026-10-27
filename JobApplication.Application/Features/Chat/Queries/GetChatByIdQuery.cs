using JobApplication.Application.Features.Chat.DTOs;
using JobApplication.Application.Patterns;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Chat.Queries
{
    public sealed record GetChatByIdQuery(Guid ChatId) : IRequest<GenericResult<ChatResponseDTO>>;
}
