using JobApplication.Application.Features.Auth.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;

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
}
