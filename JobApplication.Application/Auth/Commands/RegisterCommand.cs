using JobApplication.Application.Auth.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Auth.Commands
{
    public sealed record RegisterCommand(RegisterRequestDTO Request) : IRequest<GenericResult<RegisterResponseDTO>>;
}
