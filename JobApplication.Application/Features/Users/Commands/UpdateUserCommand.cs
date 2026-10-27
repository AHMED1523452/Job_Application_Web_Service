using JobApplication.Application.Features.Users.DTOs;
using MediatR;
using JobApplication.Application.Patterns;

namespace JobApplication.Application.Features.Users.Commands
{
    public record UpdateUserCommand(
        Guid UserId,
        UpdateUserRequestDTO Request
    ) : IRequest<GenericResult<UserResponseDTO>>;
}
