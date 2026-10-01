using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Patterns;
using MediatR;
using System;

namespace JobApplication.Application.Features.Jobs.Commands
{
    public sealed record UpdateJobCommand(Guid Id, JobRequestDTO Request) : IRequest<GenericResult<JobDetailsResponseDTO>>;
}
