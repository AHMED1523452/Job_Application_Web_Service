using JobApplication.Application.Features.Jobs.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;
using JobEntity = JobApplication.Domain.Entities.Jobs;
namespace JobApplication.Application.Features.Jobs.Handlers;
public sealed class RecordJobViewHandler : IRequestHandler<RecordJobViewCommand, GenericResult<int>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _user;
    public RecordJobViewHandler(IUnitOfWork uow,
                                ICurrentUserService user)
    {
        _uow=uow;
        _user=user;
    }
    public async Task<GenericResult<int>> Handle(RecordJobViewCommand request,CancellationToken ct)
    {
        var job=await _uow.Repository<JobEntity>()
            .GetByIdAsync(x => x.Id == request.Id&&x.jobStatus==JobStatus.Published && !x.IsDeleted && !x.IsArchived
            &&(!x.ApplicationDeadlineUtc.HasValue || x.ApplicationDeadlineUtc > DateTime.UtcNow),ct);
        if(job is null)
            return await Result.FailureAsync<int>("Job not found or is no longer accepting applications.");
        var userId=_user.GetCurrentUserIdOrNull();

        //. is the user is authenticated or is the current user is HR or not 
        if(!userId.HasValue||!await _uow.Repository<HrProfile>().AnyAsync(x => x.Id == job.HrProfileId && x.UserId == userId.Value,ct))
        {
            await _uow.Repository<JobView>().AddAsync(new JobView{Id=Guid.NewGuid(),JobId=job.Id,UserId=userId,ViewedAt=DateTime.UtcNow,IpAddress=_user.UserIP(),UserAgent=_user.UserAgent()},ct);
            await _uow.SaveChangesAsync(ct);
        }
        var count=(
            await _uow.Repository<JobView>().GetAllAsync<JobView>(x=>x.JobId==job.Id,cancellationToken:ct)
            ).Count();
        return await Result.SuccessAsync(count,"Job view recorded.");
    }
}
