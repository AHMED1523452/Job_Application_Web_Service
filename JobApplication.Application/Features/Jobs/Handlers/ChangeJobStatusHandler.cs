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
public sealed class ChangeJobStatusHandler : IRequestHandler<ChangeJobStatusCommand, GenericResult<JobDetailsResponseDTO>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _user; 
    private readonly IMapper _mapper; 
    private readonly IBackgroundJobService _backgroundJobs;
    public ChangeJobStatusHandler(IUnitOfWork uow, 
                                  ICurrentUserService user, 
                                  IMapper mapper, 
                                  IBackgroundJobService backgroundJobs) 
    { 
        _uow=uow; _user=user; 
        _mapper=mapper;
        _backgroundJobs=backgroundJobs; 
    }
    public async Task<GenericResult<JobDetailsResponseDTO>> Handle(ChangeJobStatusCommand request, CancellationToken ct)
    {
        var userId=_user.GetCurrentUserIdOrNull();
        var job=await _uow.Repository<JobEntity>().GetByIdAsyncWithTracking(x=>x.Id==request.Id&&!x.IsDeleted&&!x.IsArchived,ct,x=>x.HrProfile,x=>x.JobCategory,x=>x.JobRequirements);
        if(job is null)
            return await Result.FailureAsync<JobDetailsResponseDTO>("Job not found.");

        if(!userId.HasValue||job.HrProfile.UserId!=userId) return await Result.FailureAsync<JobDetailsResponseDTO>("You can only change your own jobs.");

        var next=request.Request.Status;
        //. Validate the status first 
        var allowed=job.jobStatus
                    switch
                    {
                        JobStatus.Draft=>next is JobStatus.Published or JobStatus.Cancelled, 
                        JobStatus.Published=>next is JobStatus.Closed or JobStatus.Cancelled, 
                        JobStatus.Closed=>next is JobStatus.Published or JobStatus.Cancelled, 
                        _=>false 
                    };

        if(!allowed) 
            return await Result.FailureAsync<JobDetailsResponseDTO>($"Cannot change job status from {job.jobStatus} to {next}.");

        //. Validate the Application Deadline is existing or the DateTime more than current and the status too 
        if( next==JobStatus.Published&& job.ApplicationDeadlineUtc.HasValue && job.ApplicationDeadlineUtc <= DateTime.UtcNow) 
                return await Result.FailureAsync<JobDetailsResponseDTO>("A job with an expired deadline cannot be published.");

        job.jobStatus=next; 

        //. Another validating on the status from the request 
        job.IsActive= (next==JobStatus.Published);
        job.UpdatedAt=DateTime.UtcNow; 
        job.UpdatedBy=userId.Value.ToString(); 
        job.UpdatedByIp=_user.UserIP();

        await _uow.Repository<JobEntity>().EditAsync(job,ct); 
        await _uow.SaveChangesAsync(ct);

        //. Using the Background job service to create a schedule 
        if(next == JobStatus.Published && job.ApplicationDeadlineUtc.HasValue)
        {
            var delay = job.ApplicationDeadlineUtc.Value - DateTime.UtcNow;

            //. if the delay has a value or more than 0 the delay will be existed if not set it as Timespan.Zero 
            await _backgroundJobs.Schedule<IJobDeadlineService>(service=>service.CloseIfExpiredAsync(job.Id), delay > TimeSpan.Zero ? delay:TimeSpan.Zero);
        }
        return await Result.SuccessAsync(await JobHandlerHelpers.ToDetailsAsync(_uow,_mapper,job,userId,ct),"Job status updated.");
    }
}
