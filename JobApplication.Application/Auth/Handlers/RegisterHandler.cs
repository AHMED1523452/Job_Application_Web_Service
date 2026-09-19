using JobApplication.Application.Auth.Commands;
using JobApplication.Application.Auth.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Auth.Handlers
{
    public class RegisterHandler : IRequestHandler<RegisterCommand, GenericResult<RegisterResponseDTO>>
    {
        private readonly IAuthService _authService;

        public RegisterHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public Task<GenericResult<RegisterResponseDTO>> Handle(RegisterCommand request, CancellationToken cancellationToken)
            => _authService.RegisterAsync(request.Request, cancellationToken);
    }
}

