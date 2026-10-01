using JobApplication.Application.Features.Notifications.DTOs;
using JobApplication.Application.Features.Notifications.Mapping;
using JobApplication.Application.Features.Notifications.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Notifications.Handlers;

public sealed class GetMyNotificationsHandler
    : IRequestHandler<GetMyNotificationsQuery, GenericResult<PaginatedResult<NotificationResponseDTO>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetMyNotificationsHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<GenericResult<PaginatedResult<NotificationResponseDTO>>> Handle(
        GetMyNotificationsQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<PaginatedResult<NotificationResponseDTO>>(
                PaginatedResultMessages.AuthenticationRequired);

        var (entities, totalCount) = await _unitOfWork.Repository<Notification>()
            .GetPagedAsync<Notification>(
                query.Page,
                query.PageSize,
                notification => notification.UserId == userId.Value
                    && !notification.IsDeleted
                    && !notification.IsArchived
                    && (!query.UnreadOnly.HasValue || notification.IsRead == !query.UnreadOnly.Value),
                notification => notification,
                orderBy: nameof(Notification.CreatedAt),
                orderByDescending: true,
                cancellationToken: cancellationToken);

        //. without calling the Eager loading , this making us selecting specific columns using the Explicit mapping 
       List<NotificationResponseDTO> items = entities.Select(NotificationMapper.ToResponse).ToList();

        var page = new PaginatedResult<NotificationResponseDTO>
        {
            Message = PaginatedResultMessages.NotificationsLoaded,
            IsSuccess = true,
            Items = items,
            Pagination = new PaginationMetadata
            {
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
            }
        };

        return await Result.SuccessAsync(page, ResultMessages.NotificationsLoaded);
    }
}