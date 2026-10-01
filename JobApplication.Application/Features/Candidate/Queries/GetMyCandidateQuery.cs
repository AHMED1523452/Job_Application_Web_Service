using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Candidate.Queries;

public sealed record GetMyCandidateQuery : IRequest<GenericResult<CandidateResponseDTO>>;
