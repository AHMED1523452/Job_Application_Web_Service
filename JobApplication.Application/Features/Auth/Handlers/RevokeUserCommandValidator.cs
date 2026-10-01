using FluentValidation;
using JobApplication.Application.Features.Auth.Commands;

namespace JobApplication.Application.Features.Auth.Handlers
{
    public class RevokeUserCommandValidator : AbstractValidator<RevokeUserCommand>
    {
        public RevokeUserCommandValidator()
        {
            RuleFor(x => x.Request.UserId)
                .NotEmpty().WithMessage("User id is required.");
        }
    }
}
