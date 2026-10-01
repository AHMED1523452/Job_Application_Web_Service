using JobApplication.Application.Features.Auth.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;

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
}
