using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Patterns;
using MediatR;
using System;

namespace JobApplication.Application.Features.Candidate.Commands
{
    public sealed record UpdateCandidateCommand(Guid Id, CandidateRequestDTO Request) : IRequest<GenericResult<CandidateResponseDTO>>;
}
