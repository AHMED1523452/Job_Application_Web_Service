using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Patterns;
using MediatR;
using System;

namespace JobApplication.Application.Features.Jobs.Queries
{
    public sealed record GetJobByIdQuery(Guid Id) : IRequest<GenericResult<JobDetailsResponseDTO>>;
}
