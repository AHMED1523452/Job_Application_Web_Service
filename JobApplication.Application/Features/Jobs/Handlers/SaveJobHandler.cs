using FluentValidation;
using JobApplication.Application.Features.Jobs.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;
using JobEntity = JobApplication.Domain.Entities.Jobs;

namespace JobApplication.Application.Features.Jobs.Handlers;

public sealed class SaveJobHandler : IRequestHandler<SaveJobCommand, GenericResult<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public SaveJobHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<GenericResult<Guid>> Handle(SaveJobCommand command, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<Guid>("Authentication is required.");

        var candidateProfile = await _unitOfWork.Repository<CandidateProfile>().GetByIdAsNoTrackingAsync<CandidateProfile>(
            p => p.UserId == userId.Value && !p.IsDeleted && !p.IsArchived && p.IsActive,
            null, ct);

        if (candidateProfile is null)
            return await Result.FailureAsync<Guid>("Candidate profile not found.");

        var job = await _unitOfWork.Repository<JobEntity>().GetByIdAsync(
            j => j.Id == command.JobId && !j.IsDeleted && !j.IsArchived && j.IsActive,
            ct);

        if (job is null)
            return await Result.FailureAsync<Guid>("Job not found.");

        var existing = await _unitOfWork.Repository<SavedJobs>().GetByIdAsNoTrackingAsync<SavedJobs>(
            s => s.JobId == command.JobId && s.CandidateProfileId == candidateProfile.Id,
            null, ct);

        if (existing is not null)
            return await Result.FailureAsync<Guid>("Job already saved.");

        var savedJob = new SavedJobs
        {
            Id = Guid.NewGuid(),
            JobId = command.JobId,
            CandidateProfileId = candidateProfile.Id,
            SavedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<SavedJobs>().AddAsync(savedJob, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await Result.SuccessAsync(savedJob.Id, "Job saved successfully.");
    }
}

public class SaveJobCommandValidator : AbstractValidator<SaveJobCommand>
{
    public SaveJobCommandValidator()
    {
        RuleFor(x => x.JobId).NotEmpty();
    }
}