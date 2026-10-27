using FluentValidation;
using JobApplication.Application.Features.Auth.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Auth.Handlers
{
    public sealed class UnRevokeUserHandler : IRequestHandler<UnRevokeUserCommand, GenericResult<string>>
    {
        private readonly IAuthService _authService;

        public UnRevokeUserHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public Task<GenericResult<string>> Handle(UnRevokeUserCommand request, CancellationToken cancellationToken)
            => _authService.UnRevokeUserAsync(request.Request, cancellationToken);
    }

    public class UnRevokeUserCommandValidator : AbstractValidator<UnRevokeUserCommand>
    {
        public UnRevokeUserCommandValidator()
        {
            RuleFor(x => x.Request.UserId)
                .NotEmpty().WithMessage("User id is required.");
        }
    }
}
