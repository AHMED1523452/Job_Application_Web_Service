using FluentValidation;
using JobApplication.Application.Features.Jobs.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;
using JobEntity = JobApplication.Domain.Entities.Jobs;

namespace JobApplication.Application.Features.Jobs.Handlers;

public sealed class RecordJobViewHandler : IRequestHandler<RecordJobViewCommand, GenericResult<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RecordJobViewHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<GenericResult<int>> Handle(RecordJobViewCommand command, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<int>("Authentication is required.");

        var job = await _unitOfWork.Repository<JobEntity>().GetByIdAsNoTrackingAsync<JobEntity>(
            j => j.Id == command.Id && !j.IsDeleted && !j.IsArchived,
            null, ct);

        if (job is null)
            return await Result.FailureAsync<int>("Job not found.");

        var existingView = await _unitOfWork.Repository<JobView>().GetByIdAsNoTrackingAsync<JobView>(
            v => v.JobId == command.Id,
            null, ct);

        if (existingView is not null)
            return await Result.SuccessAsync(1, "Job view already recorded.");

        var jobView = new JobView
        {
            Id = Guid.NewGuid(),
            JobId = command.Id,
            ViewedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<JobView>().AddAsync(jobView, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await Result.SuccessAsync(1, "Job view recorded.");
    }
}

public class RecordJobViewCommandValidator : AbstractValidator<RecordJobViewCommand>
{
    public RecordJobViewCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}