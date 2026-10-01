using JobApplication.Application.Features.Auth.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Auth.Commands
{
    public sealed record RefreshTokenCommand(RefreshTokenRequestDTO Request) : IRequest<GenericResult<LoginResponseDTO>>;
}
