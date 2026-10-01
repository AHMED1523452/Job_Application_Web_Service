using FluentValidation;
using JobApplication.Application.Features.Auth.Commands;

namespace JobApplication.Application.Features.Auth.Handlers
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Request.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");

            RuleFor(x => x.Request.Code)
                .NotEmpty().WithMessage("Reset code is required.")
                .Length(6).WithMessage("Reset code must be 6 digits.");

            RuleFor(x => x.Request.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(8).WithMessage("New password must be at least 8 characters.");

            RuleFor(x => x.Request.ConfirmNewPassword)
                .Equal(x => x.Request.NewPassword).WithMessage("New password and confirm new password must match.")
                .NotEmpty().WithMessage("Confirm new password is required.")
                .MinimumLength(8).WithMessage("New password must be at least 8 characters.");
        }
    }
}
