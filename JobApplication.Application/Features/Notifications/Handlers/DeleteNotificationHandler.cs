using FluentValidation;
using JobApplication.Application.Features.Notifications.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Notifications.Handlers;

public sealed class DeleteNotificationHandler : IRequestHandler<DeleteNotificationCommand, GenericResult<long>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteNotificationHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<GenericResult<long>> Handle(DeleteNotificationCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<long>(ResultMessages.AuthenticationRequired);

        var repository = _unitOfWork.Repository<Notification>();
        var notification = await repository.GetByIdAsyncWithTracking(
            item => item.Id == command.NotificationId
                && item.UserId == userId.Value
                && !item.IsDeleted
                && !item.IsArchived,
            cancellationToken);

        if (notification is null)
            return await Result.FailureAsync<long>(ResultMessages.NotificationNotFound);

        notification.IsDeleted = true;
        notification.DeletedAt = DateTime.UtcNow;
        notification.DeletedBy = userId.Value.ToString();
        notification.UpdatedAt = DateTime.UtcNow;
        notification.UpdatedBy = userId.Value.ToString();

        await repository.EditAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await Result.SuccessAsync(notification.Id, ResultMessages.NotificationDeleted);
    }
}

public class DeleteNotificationCommandValidator : AbstractValidator<DeleteNotificationCommand>
{
    public DeleteNotificationCommandValidator() => RuleFor(command => command.NotificationId).GreaterThan(0);
}
