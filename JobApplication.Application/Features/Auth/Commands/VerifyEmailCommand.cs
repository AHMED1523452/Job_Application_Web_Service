using JobApplication.Application.Features.Auth.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Auth.Commands
{
    public sealed record VerifyEmailCommand(VerifyEmailRequestDTO Request) : IRequest<GenericResult<string>>;
}
