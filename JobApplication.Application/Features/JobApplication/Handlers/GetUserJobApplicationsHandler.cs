using FluentValidation;
using System.Linq.Expressions;
using JobApplication.Application.Features.JobApplication.DTOs;
using JobApplication.Application.Features.JobApplication.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.JobApplication.Handlers;

public sealed class GetUserJobApplicationsHandler
    : IRequestHandler<GetUserJobApplicationsQuery, GenericResult<PaginatedResult<JobApplicationResponseDTO>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IApplicationCache _cache;

    public GetUserJobApplicationsHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IApplicationCache cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<GenericResult<PaginatedResult<JobApplicationResponseDTO>>> Handle(
        GetUserJobApplicationsQuery query,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<PaginatedResult<JobApplicationResponseDTO>>(PaginatedResultMessages.AuthenticationRequired);

        var ownsProfile = await _unitOfWork.Repository<CandidateProfile>().AnyAsync(
            candidate => candidate.Id == query.CandidateProfileId
                && candidate.UserId == userId.Value
                && candidate.IsActive
                && !candidate.IsArchived
                && !candidate.IsDeleted,
            cancellationToken);
        if (!ownsProfile)
            return await Result.FailureAsync<PaginatedResult<JobApplicationResponseDTO>>("The candidate profile in the URL does not belong to the authenticated user.");

        var cacheKey = $"job-application:candidate:{query.CandidateProfileId}:user:{userId.Value}:page:{query.Page}:pageSize:{query.PageSize}:search:{Uri.EscapeDataString(query.Search?.Trim().ToLowerInvariant() ?? string.Empty)}:sort:{query.OrderBy}:{query.OrderDesc}";
        var cached = await _cache.GetAsync<GenericResult<PaginatedResult<JobApplicationResponseDTO>>>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        Expression<Func<JobApplications, bool>> predicate = application =>
            application.CandidateProfileId == query.CandidateProfileId
            && !application.IsDeleted;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            predicate = application => application.CandidateProfileId == query.CandidateProfileId
                && !application.IsDeleted
                && ((application.Job != null && application.Job.Title.Contains(search))
                    || (application.CoverLetter != null && application.CoverLetter.Contains(search)));
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
                    JobTitle = application.Job == null ? null : application.Job.Title,
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
        var response = await Result.SuccessAsync(page, ResultMessages.PaginatedQueryCompleted);
        await _cache.CreateAsync(cacheKey, response, cancellationToken);
        return response;
    }
}

public class GetUserJobApplicationsQueryValidator : AbstractValidator<GetUserJobApplicationsQuery>
{
    public GetUserJobApplicationsQueryValidator()
    {
        RuleFor(query => query.CandidateProfileId).NotEmpty();
        RuleFor(query => query.Page).GreaterThan(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Search).MaximumLength(100);
        RuleFor(query => query.OrderBy)
            .Must(orderBy => string.IsNullOrWhiteSpace(orderBy)
                || new[] { "AppliedAtUtc", "Id", "Status" }.Contains(orderBy, StringComparer.OrdinalIgnoreCase))
            .WithMessage("OrderBy must be AppliedAtUtc, Id, or Status.");
    }
}
