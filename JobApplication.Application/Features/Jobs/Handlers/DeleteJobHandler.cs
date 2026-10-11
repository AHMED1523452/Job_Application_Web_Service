using JobApplication.Application.Features.Jobs.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;
using JobEntity = JobApplication.Domain.Entities.Jobs;
namespace JobApplication.Application.Features.Jobs.Handlers;
public sealed class DeleteJobHandler : IRequestHandler<DeleteJobCommand, GenericResult<Guid>>
{
    private readonly IUnitOfWork _uow; 
    private readonly ICurrentUserService _user;
    private readonly IApplicationCache _cache;
    public DeleteJobHandler(IUnitOfWork uow, ICurrentUserService user, IApplicationCache cache)
    {
        _uow=uow;
        _user=user;
        _cache=cache;
    }
    public async Task<GenericResult<Guid>> Handle(DeleteJobCommand request,CancellationToken ct)
    {
        var userId=_user.GetCurrentUserIdOrNull();
        var job=await _uow.Repository<JobEntity>().GetByIdAsyncWithTracking(x=>x.Id==request.Id&&!x.IsDeleted&&!x.IsArchived,ct,x=>x.HrProfile);

        if(job is null)
            return await Result.FailureAsync<Guid>("Job not found.");

        if(!userId.HasValue||job.HrProfile.UserId!=userId)
            return await Result.FailureAsync<Guid>("You can only archive your own jobs.");

        job.IsArchived=true;
        job.IsActive=false;
        job.jobStatus=Domain.Enums.JobStatus.Cancelled;
        job.UpdatedAt=DateTime.UtcNow;
        job.UpdatedBy=userId.Value.ToString();
        job.UpdatedByIp=_user.UserIP();

        await _uow.Repository<JobEntity>().EditAsync(job,ct);
        await _uow.SaveChangesAsync(ct);
        await _cache.InvalidateAsync("job:", ct);
        await _cache.InvalidateAsync("job-application:", ct);

        return await Result.SuccessAsync(job.Id,"Job archived successfully.");
    }
}
