using JobApplication.Application.Features.Notifications.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Notifications.Handlers;

public sealed class GetUnreadNotificationCountHandler
    : IRequestHandler<GetUnreadNotificationCountQuery, GenericResult<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetUnreadNotificationCountHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<GenericResult<int>> Handle(GetUnreadNotificationCountQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<int>(ResultMessages.AuthenticationRequired);

        var count = await _unitOfWork.Repository<Notification>().CountAsync(
            notification => notification.UserId == userId.Value
                && !notification.IsDeleted
                && !notification.IsArchived
                && !notification.IsRead,
            cancellationToken);

        return await Result.SuccessAsync(count, "Unread notification count loaded successfully.");
    }
}
