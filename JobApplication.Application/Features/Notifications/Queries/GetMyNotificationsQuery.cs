using JobApplication.Application.Features.Notifications.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Notifications.Queries;

public sealed record GetMyNotificationsQuery(int Page = 1, int PageSize = 20, bool? UnreadOnly = null)
    : IRequest<GenericResult<PaginatedResult<NotificationResponseDTO>>>;
