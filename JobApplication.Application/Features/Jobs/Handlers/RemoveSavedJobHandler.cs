using FluentValidation;
using JobApplication.Application.Features.Jobs.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Handlers;

public sealed class RemoveSavedJobHandler : IRequestHandler<RemoveSavedJobCommand, GenericResult<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RemoveSavedJobHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<GenericResult<Guid>> Handle(RemoveSavedJobCommand command, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<Guid>("Authentication is required.");

        var candidateProfile = await _unitOfWork.Repository<CandidateProfile>().GetByIdAsNoTrackingAsync<CandidateProfile>(
            p => p.UserId == userId.Value && !p.IsDeleted && !p.IsArchived && p.IsActive,
            null, ct);

        if (candidateProfile is null)
            return await Result.FailureAsync<Guid>("Candidate profile not found.");

        var savedJob = await _unitOfWork.Repository<SavedJobs>().GetByIdAsNoTrackingAsync<SavedJobs>(
            s => s.Id == command.Id && s.CandidateProfileId == candidateProfile.Id,
            null, ct);

        if (savedJob is null)
            return await Result.FailureAsync<Guid>("Saved job not found.");

        await _unitOfWork.Repository<SavedJobs>().RemoveAsync(savedJob, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await Result.SuccessAsync(savedJob.Id, "Job removed from saved.");
    }
}

public class RemoveSavedJobCommandValidator : AbstractValidator<RemoveSavedJobCommand>
{
    public RemoveSavedJobCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}