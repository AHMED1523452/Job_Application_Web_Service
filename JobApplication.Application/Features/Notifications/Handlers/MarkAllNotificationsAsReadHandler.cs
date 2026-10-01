using JobApplication.Application.Features.Notifications.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Notifications.Handlers;

public sealed class MarkAllNotificationsAsReadHandler
    : IRequestHandler<MarkAllNotificationsAsReadCommand, GenericResult<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public MarkAllNotificationsAsReadHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<GenericResult<int>> Handle(MarkAllNotificationsAsReadCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<int>(ResultMessages.AuthenticationRequired);

        var repository = _unitOfWork.Repository<Notification>();
        var now = DateTime.UtcNow;
        var updatedBy = userId.Value.ToString();

        //. Avoiding the for loop and updating the records one by one and there is loading in memory 
        
        var count = await repository.ExecuteUpdateAsync(
            notification =>
             notification.UserId == userId.Value
                && !notification.IsDeleted
                && !notification.IsArchived
                && !notification.IsRead,
            setters => setters
                .SetProperty(notification => notification.IsRead, true)
                .SetProperty(notification => notification.ReadAtUtc, now)
                .SetProperty(notification => notification.UpdatedAt, now)
                .SetProperty(notification => notification.UpdatedBy, updatedBy),
            cancellationToken);


        return await Result.SuccessAsync(count, ResultMessages.NotificationsMarkedRead);
    }
}
