using JobApplication.Application.Features.Auth.Commands;
using JobApplication.Application.Features.Auth.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Auth.Handlers
{
    public sealed class RegisterRecruiterHandler
        : IRequestHandler<RegisterRecruiterCommand, GenericResult<RegisterResponseDTO>>
    {
        private readonly IAuthService _authService;

        public RegisterRecruiterHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public Task<GenericResult<RegisterResponseDTO>> Handle(
            RegisterRecruiterCommand request,
            CancellationToken cancellationToken)
            => _authService.RegisterRecruiterAsync(request.Request, cancellationToken);
    }
}
