using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Queries
{
    public sealed record GetJobsQuery(JobSearchRequestDTO Request, int Page = 1, int PageSize = 10)
        : IRequest<PaginatedResult<JobListItemsResponseDTO>>;
}
