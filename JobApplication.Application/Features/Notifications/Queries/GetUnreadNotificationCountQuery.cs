using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Notifications.Queries;

public sealed record GetUnreadNotificationCountQuery : IRequest<GenericResult<int>>;
