using AutoMapper;
using JobApplication.Application.Features.Jobs.Commands;
using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;
using JobEntity = JobApplication.Domain.Entities.Jobs;
namespace JobApplication.Application.Features.Jobs.Handlers;
public sealed class SaveJobHandler : IRequestHandler<SaveJobCommand, GenericResult<JobListItemsResponseDTO>>
{
    private readonly IUnitOfWork _uow; 
    private readonly ICurrentUserService _user; 
    private readonly IMapper _mapper;
    public SaveJobHandler(IUnitOfWork uow,
                          ICurrentUserService user, 
                          IMapper mapper)
    {
        _uow=uow;
        _user=user;
        _mapper=mapper;
    }
    public async Task<GenericResult<JobListItemsResponseDTO>> Handle(SaveJobCommand request,CancellationToken ct)
    {
        var userId=_user.GetCurrentUserIdOrNull(); 
        if(!userId.HasValue)
            return await Result.FailureAsync<JobListItemsResponseDTO>("Authentication is required.");
        var profile=await _uow.Repository<CandidateProfile>().GetByIdAsync(x=>x.UserId==userId.Value,ct);
        if(profile is null)
            return await Result.FailureAsync<JobListItemsResponseDTO>("Create a candidate profile before saving jobs.");

        var job=await _uow.Repository<JobEntity>().GetByIdAsyncWithTracking(x=>x.Id==request.Id&&x.jobStatus==JobStatus.Published&&!x.IsDeleted&&!x.IsArchived&&(!x.ApplicationDeadlineUtc.HasValue||x.ApplicationDeadlineUtc>DateTime.UtcNow),ct,x=>x.HrProfile,x=>x.JobCategory);
        if(job is null)
            return await Result.FailureAsync<JobListItemsResponseDTO>("Only active jobs accepting applications can be saved.");

        var saved=await _uow.Repository<SavedJobs>().GetByIdAsyncWithTracking(x=>x.CandidateProfileId==profile.Id&&x.JobId==job.Id,ct);
        if(saved is null)
        {
            saved = new SavedJobs
            {
                Id = Guid.NewGuid(),
                CandidateProfileId = profile.Id,
                JobId = job.Id,
                SavedAt = DateTime.UtcNow
            };
            await _uow.Repository<SavedJobs>().AddAsync(saved,ct);
        }
        saved.Notes=request.Request.Notes?.Trim(); if(saved.Id!=Guid.Empty)await _uow.Repository<SavedJobs>().EditAsync(saved,ct);
        await _uow.SaveChangesAsync(ct);
        var dto=_mapper.Map<JobListItemsResponseDTO>(job);dto.IsSavedByCurrentUser=true;
        return await Result.SuccessAsync(dto,"Job saved.");
    }
}
