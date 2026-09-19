using JobApplication.Application.JobApplication.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.JobApplication.Commands
{
    public sealed record UpdateJobApplicationCommand(Guid id,UpdateJobApplicationRequestDTO requestDTO) : IRequest<GenericResult<UpdateApplicationResponseDTO>>;
}
