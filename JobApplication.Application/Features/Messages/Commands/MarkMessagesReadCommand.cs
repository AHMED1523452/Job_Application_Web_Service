using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Messages.Commands;

public sealed record MarkMessagesReadCommand(Guid ChatId) : IRequest<GenericResult<int>>;
