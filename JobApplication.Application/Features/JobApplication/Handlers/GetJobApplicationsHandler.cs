using System.Linq.Expressions;
using JobApplication.Application.Features.JobApplication.DTOs;
using JobApplication.Application.Features.JobApplication.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;
using JobEntity = JobApplication.Domain.Entities.Jobs;

namespace JobApplication.Application.Features.JobApplication.Handlers;

public sealed class GetJobApplicationsHandler
    : IRequestHandler<GetJobApplicationsQuery, GenericResult<PaginatedResult<JobApplicationResponseDTO>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetJobApplicationsHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<GenericResult<PaginatedResult<JobApplicationResponseDTO>>> Handle(
        GetJobApplicationsQuery query,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<PaginatedResult<JobApplicationResponseDTO>>(PaginatedResultMessages.AuthenticationRequired);

        var ownsJob = await _unitOfWork.Repository<JobEntity>().AnyAsync(
            job => job.Id == query.JobId
                && job.HrProfile.UserId == userId.Value
                && !job.IsDeleted
                && !job.IsArchived,
            cancellationToken);
        if (!ownsJob)
            return await Result.FailureAsync<PaginatedResult<JobApplicationResponseDTO>>("The job was not found or does not belong to the authenticated recruiter.");

        Expression<Func<JobApplications, bool>> predicate = application =>
            application.JobId == query.JobId && !application.IsDeleted;
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            predicate = application => application.JobId == query.JobId
                && !application.IsDeleted
                && (application.CoverLetter != null && application.CoverLetter.Contains(search));
        }

        var (items, totalCount) = await _unitOfWork.Repository<JobApplications>()
            .GetPagedAsync<JobApplicationResponseDTO>(
                query.Page,
                query.PageSize,
                predicate,
                application => new JobApplicationResponseDTO
                {
                    Id = application.Id,
                    JobId = application.JobId,
                    JobTitle = application.Job.Title,
                    Description = application.CoverLetter ?? string.Empty,
                    Status = application.Status.ToString(),
                    CandidateId = application.CandidateProfileId,
                    CreatedAt = application.AppliedAtUtc
                },
                orderBy: string.IsNullOrWhiteSpace(query.OrderBy) ? nameof(JobApplications.AppliedAtUtc) : query.OrderBy,
                orderByDescending: query.OrderDesc,
                cancellationToken: cancellationToken);

        var page = new PaginatedResult<JobApplicationResponseDTO>
        {
            Message = PaginatedResultMessages.Loaded,
            IsSuccess = true,
            Items = items,
            Pagination = new PaginationMetadata
            {
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
            }
        };
        return await Result.SuccessAsync(page, ResultMessages.PaginatedQueryCompleted);
    }
}
