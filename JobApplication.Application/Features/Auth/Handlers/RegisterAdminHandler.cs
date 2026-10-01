using JobApplication.Application.Features.Auth.Commands;
using JobApplication.Application.Features.Auth.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Auth.Handlers
{
    public sealed class RegisterAdminHandler
        : IRequestHandler<RegisterAdminCommand, GenericResult<RegisterResponseDTO>>
    {
        private readonly IAuthService _authService;

        public RegisterAdminHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public Task<GenericResult<RegisterResponseDTO>> Handle(
            RegisterAdminCommand request,
            CancellationToken cancellationToken)
            => _authService.RegisterAdminAsync(request.Request, cancellationToken);
    }
}
