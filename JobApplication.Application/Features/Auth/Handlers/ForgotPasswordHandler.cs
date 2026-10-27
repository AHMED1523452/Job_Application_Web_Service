using FluentValidation;
using JobApplication.Application.Features.Auth.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Auth.Handlers
{
    public sealed class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, GenericResult<string>>
    {
        private readonly IAuthService _authService;

        public ForgotPasswordHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public Task<GenericResult<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
            => _authService.ForgotPasswordAsync(request.Request, cancellationToken);
    }

    public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(x => x.Request.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");
        }
    }
}
