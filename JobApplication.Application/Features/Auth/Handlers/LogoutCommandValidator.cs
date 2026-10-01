using FluentValidation;
using JobApplication.Application.Features.Auth.Commands;

namespace JobApplication.Application.Features.Auth.Handlers
{
    public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
    {
        public LogoutCommandValidator()
        {
            RuleFor(x => x.Request.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.");
        }
    }
}
