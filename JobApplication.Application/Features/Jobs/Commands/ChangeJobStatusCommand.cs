using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands
{
    public sealed record ChangeJobStatusCommand(Guid Id, ChangeJobStatusRequestDTO Request)
        : IRequest<GenericResult<JobDetailsResponseDTO>>;
}
