using FluentValidation;
using JobApplication.Application.Features.Auth.Commands;

namespace JobApplication.Application.Features.Auth.Handlers;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(command => command.Request).NotNull();
        When(command => command.Request is not null, () =>
        {
            RuleFor(command => command.Request.CurrentPassword).NotEmpty();
            RuleFor(command => command.Request.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .Matches("[A-Z]").WithMessage("New password must contain an uppercase letter.")
                .Matches("[a-z]").WithMessage("New password must contain a lowercase letter.")
                .Matches("[0-9]").WithMessage("New password must contain a digit.");
            RuleFor(command => command.Request.ConfirmNewPassword)
                .Equal(command => command.Request.NewPassword)
                .WithMessage("New password and confirmation must match.");
            RuleFor(command => command.Request.NewPassword)
                .NotEqual(command => command.Request.CurrentPassword)
                .WithMessage("New password must differ from the current password.");
        });
    }
}
