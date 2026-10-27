using FluentValidation;
using JobApplication.Application.Features.Auth.Commands;
using JobApplication.Application.Features.Auth.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Auth.Handlers
{
    public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, GenericResult<LoginResponseDTO>>
    {
        private readonly IAuthService _authService;

        public RefreshTokenHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public Task<GenericResult<LoginResponseDTO>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
            => _authService.RefreshTokenAsync(request.Request, cancellationToken);
    }

    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.Request.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.");
        }
    }
}
