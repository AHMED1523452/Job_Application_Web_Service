using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Patterns;
using MediatR;
using System;

namespace JobApplication.Application.Features.Candidate.Queries
{
    public sealed record GetCandidateByIdQuery(Guid Id) : IRequest<GenericResult<CandidateResponseDTO>>;
}
