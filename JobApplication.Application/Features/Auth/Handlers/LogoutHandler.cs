using FluentValidation;
using JobApplication.Application.Features.Auth.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Auth.Handlers
{
    public sealed class LogoutHandler : IRequestHandler<LogoutCommand, GenericResult<string>>
    {
        private readonly IAuthService _authService;

        public LogoutHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public Task<GenericResult<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
            => _authService.LogoutAsync(request.Request, cancellationToken);
    }

    public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
    {
        public LogoutCommandValidator()
        {
            RuleFor(x => x.Request.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.");
        }
    }
}
