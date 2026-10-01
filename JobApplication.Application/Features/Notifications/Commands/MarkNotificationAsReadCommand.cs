using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Notifications.Commands;

public sealed record MarkNotificationAsReadCommand(long NotificationId) : IRequest<GenericResult<long>>;
