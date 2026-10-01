using JobApplication.Application.Features.JobApplication.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.JobApplication.Commands
{
    public sealed record UpdateJobApplicationCommand(Guid candidateProfileId, Guid id, UpdateJobApplicationRequestDTO requestDTO)
        : IRequest<GenericResult<UpdateApplicationResponseDTO>>;
}
