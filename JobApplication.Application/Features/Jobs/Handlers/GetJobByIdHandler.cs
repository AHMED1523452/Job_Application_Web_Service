using FluentValidation;
using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Features.Jobs.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;
using JobEntity = JobApplication.Domain.Entities.Jobs;

namespace JobApplication.Application.Features.Jobs.Handlers;

public sealed class GetJobByIdHandler : IRequestHandler<GetJobByIdQuery, GenericResult<JobDetailsResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IApplicationCache _cache;

    public GetJobByIdHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IApplicationCache cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<GenericResult<JobDetailsResponseDTO>> Handle(GetJobByIdQuery request, CancellationToken ct)
    {
        var currentUserId = _currentUser.GetCurrentUserIdOrNull();
        var cacheKey = $"job:by-id:{request.Id}:user:{currentUserId?.ToString() ?? "anonymous"}";
        var cached = await _cache.GetAsync<GenericResult<JobDetailsResponseDTO>>(cacheKey, ct);
        if (cached is not null) return cached;

        var projection = await _unitOfWork.Repository<JobEntity>().GetByIdAsNoTrackingAsync(
            job => job.Id == request.Id && !job.IsDeleted && !job.IsArchived,
            JobDetailsQueryProjection.Select,
            ct);

        if (projection is null)
            return await Result.FailureAsync<JobDetailsResponseDTO>("Job not found.");

        var userId = _currentUser.GetCurrentUserIdOrNull();
        var isOwner = userId.HasValue && projection.RecruiterUserId == userId.Value;

        if (projection.Details.JobStatus != JobStatus.Published && !isOwner)
            return await Result.FailureAsync<JobDetailsResponseDTO>("Job not found.");

        var details = projection.Details;
        details.Requirements = projection.Requirements.Select(requirement =>
        {
            int? minimumYears = int.TryParse(requirement.ExpectedValue, out var years) ? years : null;
            return new JobRequirementResponse
            {
                Id = requirement.Id,
                SkillId = requirement.SkillId,
                SkillName = requirement.SkillName,
                Description = requirement.Description,
                IsRequired = requirement.IsRequired,
                MinYearsExperience = minimumYears
            };
        }).ToList();

        details.ViewCount = await _unitOfWork.Repository<JobView>()
            .CountAsync(view => view.JobId == request.Id, ct);

        details.ApplicationCount = await _unitOfWork.Repository<JobApplications>()
            .CountAsync(application => application.JobId == request.Id, ct);

        if (userId.HasValue)
        {
            var candidateProfileId = await _unitOfWork.Repository<CandidateProfile>().GetByIdAsNoTrackingAsync<Guid?>(
                profile => profile.UserId == userId.Value && !profile.IsDeleted && !profile.IsArchived,
                profile => (Guid?)profile.Id,
                ct);

            if (candidateProfileId.HasValue)
            {
                details.IsSavedByCurrentUser = await _unitOfWork.Repository<SavedJobs>().AnyAsync(
                    saved => saved.JobId == request.Id
                        && saved.CandidateProfileId == candidateProfileId.Value, ct);
            }
        }

        var response = await Result.SuccessAsync(details);
        await _cache.CreateAsync(cacheKey, response, ct);
        return response;
    }
}

public class GetJobByIdQueryValidator : AbstractValidator<GetJobByIdQuery>
{
    public GetJobByIdQueryValidator() => RuleFor(x => x.Id).NotEmpty();
}
