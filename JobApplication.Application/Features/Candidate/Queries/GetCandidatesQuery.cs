using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Candidate.Queries
{
    public sealed record GetCandidatesQuery(int Page = 1, int PageSize = 10, string? Search = null, string? OrderBy = "CreatedAt", bool OrderDesc = true) : IRequest<GenericResult<PaginatedResult<CandidateResponseDTO>>>;
}
