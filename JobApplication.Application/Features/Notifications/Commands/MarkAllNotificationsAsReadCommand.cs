using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Notifications.Commands;

public sealed record MarkAllNotificationsAsReadCommand : IRequest<GenericResult<int>>;
