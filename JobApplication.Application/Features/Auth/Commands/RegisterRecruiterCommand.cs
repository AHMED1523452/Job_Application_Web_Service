using JobApplication.Application.Features.Auth.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Auth.Commands
{
    public sealed record RegisterRecruiterCommand(RecruiterRegisterRequestDTO Request)
        : IRequest<GenericResult<RegisterResponseDTO>>;
}
