using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Notifications.Commands;

public sealed record DeleteNotificationCommand(long NotificationId) : IRequest<GenericResult<long>>;
