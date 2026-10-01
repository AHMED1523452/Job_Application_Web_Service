using FluentValidation;
using JobApplication.Application.Features.Notifications.Commands;

namespace JobApplication.Application.Features.Notifications.Handlers;

public sealed class MarkNotificationAsReadCommandValidator : AbstractValidator<MarkNotificationAsReadCommand>
{
    public MarkNotificationAsReadCommandValidator() => RuleFor(command => command.NotificationId).GreaterThan(0);
}

public sealed class DeleteNotificationCommandValidator : AbstractValidator<DeleteNotificationCommand>
{
    public DeleteNotificationCommandValidator() => RuleFor(command => command.NotificationId).GreaterThan(0);
}
