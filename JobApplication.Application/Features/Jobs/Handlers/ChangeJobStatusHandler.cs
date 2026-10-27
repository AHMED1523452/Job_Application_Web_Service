using FluentValidation;
using JobApplication.Application.Features.Jobs.Commands;
using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;
using AutoMapper;
using HrProfileEntity = JobApplication.Domain.Entities.HrProfile;
using JobEntity = JobApplication.Domain.Entities.Jobs;

namespace JobApplication.Application.Features.Jobs.Handlers;

public sealed class ChangeJobStatusHandler : IRequestHandler<ChangeJobStatusCommand, GenericResult<JobDetailsResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public ChangeJobStatusHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<GenericResult<JobDetailsResponseDTO>> Handle(ChangeJobStatusCommand command, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<JobDetailsResponseDTO>("Authentication is required.");

        var job = await _unitOfWork.Repository<JobEntity>().GetByIdAsyncWithTracking(
            j => j.Id == command.Id && !j.IsDeleted && !j.IsArchived,
            ct);

        if (job is null)
            return await Result.FailureAsync<JobDetailsResponseDTO>("Job not found.");

        var recruiter = await _unitOfWork.Repository<HrProfileEntity>().GetByIdAsync(
            hr => hr.Id == job.HrProfileId && hr.UserId == userId.Value,
            ct);

        if (recruiter is null)
            return await Result.FailureAsync<JobDetailsResponseDTO>("Only the job owner can change status.");

        job.jobStatus = command.Request.Status;
        job.IsActive = command.Request.Status == JobStatus.Published;
        job.UpdatedAt = DateTime.UtcNow;
        job.UpdatedBy = userId.Value.ToString();
        job.UpdatedByIp = _currentUser.UserIP();

        await _unitOfWork.Repository<JobEntity>().EditAsync(job, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await Result.SuccessAsync(await JobHandlerHelpers.ToDetailsAsync(_unitOfWork, _mapper, job, userId, ct), "Job status changed successfully.");
    }
}

public class ChangeJobStatusCommandValidator : AbstractValidator<ChangeJobStatusCommand>
{
    public ChangeJobStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.Status).IsInEnum();
    }
}