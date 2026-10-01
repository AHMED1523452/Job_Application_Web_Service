using JobApplication.Application.Features.Auth.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Auth.Handlers
{
    public sealed class RevokeUserHandler : IRequestHandler<RevokeUserCommand, GenericResult<string>>
    {
        private readonly IAuthService _authService;

        public RevokeUserHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public Task<GenericResult<string>> Handle(RevokeUserCommand request, CancellationToken cancellationToken)
            => _authService.RevokeUserAsync(request.Request, cancellationToken);
    }
}
