using FluentValidation;
using JobApplication.Application.Features.Auth.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Auth.Handlers
{
    public sealed class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, GenericResult<string>>
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUser;

        public ChangePasswordHandler(IAuthService authService, ICurrentUserService currentUser)
        {
            _authService = authService;
            _currentUser = currentUser;
        }

        public Task<GenericResult<string>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetCurrentUserIdOrNull();
            return userId.HasValue
                ? _authService.ChangePasswordAsync(userId.Value, command.Request, cancellationToken)
                : Result.FailureAsync<string>("Authentication is required.");
        }
    }

    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(command => command.Request).NotNull();
            When(command => command.Request is not null, () =>
            {
                RuleFor(command => command.Request.CurrentPassword).NotEmpty();
                RuleFor(command => command.Request.NewPassword)
                    .NotEmpty()
                    .MinimumLength(8)
                    .Matches("[A-Z]").WithMessage("New password must contain an uppercase letter.")
                    .Matches("[a-z]").WithMessage("New password must contain a lowercase letter.")
                    .Matches("[0-9]").WithMessage("New password must contain a digit.");
                RuleFor(command => command.Request.ConfirmNewPassword)
                    .Equal(command => command.Request.NewPassword)
                    .WithMessage("New password and confirmation must match.");
                RuleFor(command => command.Request.NewPassword)
                    .NotEqual(command => command.Request.CurrentPassword)
                    .WithMessage("New password must differ from the current password.");
            });
        }
    }
}
