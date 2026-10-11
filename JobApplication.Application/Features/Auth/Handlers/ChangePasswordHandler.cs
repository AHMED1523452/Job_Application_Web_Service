using JobApplication.Application.Features.Auth.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Auth.Handlers;

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
