using FluentValidation;
using JobApplication.Application.Features.Auth.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Auth.Handlers
{
    public sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, GenericResult<string>>
    {
        private readonly IAuthService _authService;

        public ResetPasswordHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public Task<GenericResult<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
            => _authService.ResetPasswordAsync(request.Request, cancellationToken);
    }

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
