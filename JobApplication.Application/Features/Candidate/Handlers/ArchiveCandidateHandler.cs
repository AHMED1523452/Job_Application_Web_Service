using JobApplication.Application.Features.Candidate.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Candidate.Handlers;

public sealed class ArchiveCandidateHandler : IRequestHandler<ArchiveCandidateCommand, GenericResult<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IApplicationCache _cache;

    public ArchiveCandidateHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IApplicationCache cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<GenericResult<Guid>> Handle(ArchiveCandidateCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<Guid>("Authentication is required.");

        var repository = _unitOfWork.Repository<Domain.Entities.CandidateProfile>();
        var candidate = await repository.GetByIdAsyncWithTracking(
            item => item.UserId == userId.Value && !item.IsDeleted && !item.IsArchived,
            cancellationToken);

        if (candidate is null)
            return await Result.FailureAsync<Guid>("Candidate profile not found.");

        candidate.IsActive = false;
        candidate.IsArchived = true;
        candidate.UpdatedAt = DateTime.UtcNow;
        candidate.UpdatedBy = userId.Value.ToString();
        candidate.UpdatedByIp = _currentUser.UserIP();

        await repository.EditAsync(candidate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.InvalidateAsync("candidate:", cancellationToken);
        await _cache.InvalidateAsync("job-application:", cancellationToken);

        return await Result.SuccessAsync(candidate.Id, "Candidate profile archived successfully.");
    }
}
