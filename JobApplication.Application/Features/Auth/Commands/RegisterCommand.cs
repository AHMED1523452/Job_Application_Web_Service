using JobApplication.Application.Features.Auth.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Auth.Commands
{
    public sealed record RegisterCommand(RegisterRequestDTO Request) : IRequest<GenericResult<RegisterResponseDTO>>;
}
