using FluentValidation;
using JobApplication.Application.Features.JobApplication.DTOs;
using JobApplication.Application.Features.JobApplication.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.JobApplication.Handlers;

public sealed class GetCandidateJobApplicationByIdHandler
    : IRequestHandler<GetCandidateJobApplicationByIdQuery, GenericResult<JobApplicationResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IApplicationCache _cache;

    public GetCandidateJobApplicationByIdHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IApplicationCache cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<GenericResult<JobApplicationResponseDTO>> Handle(
        GetCandidateJobApplicationByIdQuery query,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<JobApplicationResponseDTO>("Authentication is required.");

        var ownsProfile = await _unitOfWork.Repository<CandidateProfile>().AnyAsync(
            candidate => candidate.Id == query.CandidateProfileId
                && candidate.UserId == userId.Value
                && candidate.IsActive
                && !candidate.IsArchived
                && !candidate.IsDeleted,
            cancellationToken);
        if (!ownsProfile)
            return await Result.FailureAsync<JobApplicationResponseDTO>(
                "The candidate profile in the URL does not belong to the authenticated user.");

        var cacheKey = $"job-application:by-id:{query.ApplicationId}:candidate:{query.CandidateProfileId}:user:{userId.Value}";
        var cached = await _cache.GetAsync<GenericResult<JobApplicationResponseDTO>>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        var application = await _unitOfWork.Repository<JobApplications>()
            .GetByIdAsNoTrackingAsync<JobApplicationResponseDTO>(
                item => item.Id == query.ApplicationId
                    && item.CandidateProfileId == query.CandidateProfileId
                    && !item.IsDeleted,
                item => new JobApplicationResponseDTO
                {
                    Id = item.Id,
                    JobId = item.JobId,
                    JobTitle = item.Job == null ? null : item.Job.Title,
                    Description = item.CoverLetter ?? string.Empty,
                    Status = item.Status.ToString(),
                    CandidateId = item.CandidateProfileId,
                    CreatedAt = item.AppliedAtUtc
                },
                cancellationToken);

        if (application is null)
            return await Result.FailureAsync<JobApplicationResponseDTO>("Job application not found.");

        var response = await Result.SuccessAsync(application);
        await _cache.CreateAsync(cacheKey, response, cancellationToken);
        return response;
    }
}

public class GetCandidateJobApplicationByIdValidator : AbstractValidator<GetCandidateJobApplicationByIdQuery>
{
    public GetCandidateJobApplicationByIdValidator()
    {
        RuleFor(query => query.CandidateProfileId).NotEmpty();
        RuleFor(query => query.ApplicationId).NotEmpty();
    }
}
