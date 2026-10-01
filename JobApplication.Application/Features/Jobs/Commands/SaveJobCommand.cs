using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands
{
    public sealed record SaveJobCommand(Guid Id, SavedJobRequestDTO Request)
        : IRequest<GenericResult<JobListItemsResponseDTO>>;
}
