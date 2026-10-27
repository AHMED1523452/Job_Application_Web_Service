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
    public sealed class LoginHandler : IRequestHandler<LoginCommand, GenericResult<LoginResponseDTO>>
    {
        private readonly IAuthService _authService;

        public LoginHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public Task<GenericResult<LoginResponseDTO>> Handle(LoginCommand request, CancellationToken cancellationToken)
            => _authService.LoginAsync(request.requestDTO, cancellationToken);
    }

    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.requestDTO.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");

            RuleFor(x => x.requestDTO.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
