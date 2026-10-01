using JobApplication.Application.Features.JobApplication.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.JobApplication.Queries
{
    public sealed record GetUserJobApplicationsQuery(Guid CandidateProfileId, int Page = 1, int PageSize = 10, string? Search = null, string? OrderBy = null, bool OrderDesc = true)
        : IRequest<GenericResult<PaginatedResult<JobApplicationResponseDTO>>>;
}
