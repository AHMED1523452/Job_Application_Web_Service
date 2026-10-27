using MediatR;
using JobApplication.Application.Patterns;

namespace JobApplication.Application.Features.Users.Commands
{
    public record DeleteUserCommand(
        Guid UserId
    ) : IRequest<GenericResult<bool>>;
}
