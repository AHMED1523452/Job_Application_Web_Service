using JobApplication.Application.Features.Jobs.Commands;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;
namespace JobApplication.Application.Features.Jobs.Handlers;
public sealed class RemoveSavedJobHandler : IRequestHandler<RemoveSavedJobCommand, GenericResult<Guid>>
{
    private readonly IUnitOfWork _uow;private readonly ICurrentUserService _user;private readonly IApplicationCache _cache;
    public RemoveSavedJobHandler(IUnitOfWork uow,ICurrentUserService user,IApplicationCache cache){_uow=uow;_user=user;_cache=cache;}
    public async Task<GenericResult<Guid>> Handle(RemoveSavedJobCommand request,CancellationToken ct)
    {
        var id=_user.GetCurrentUserIdOrNull();
        if(!id.HasValue)
            return await Result.FailureAsync<Guid>("Authentication is required.");

        var profile=await _uow.Repository<CandidateProfile>().GetByIdAsync(x=>x.UserId==id.Value,ct);
        if(profile is null)
            return await Result.FailureAsync<Guid>("Candidate profile not found.");

        var saved=await _uow.Repository<SavedJobs>().GetByIdAsyncWithTracking(x=>x.CandidateProfileId==profile.Id&&x.JobId==request.Id,ct);
        if(saved is null)
            return await Result.FailureAsync<Guid>("This job is not in your saved jobs.");

        await _uow.Repository<SavedJobs>().RemoveAsync(saved,ct);await _uow.SaveChangesAsync(ct);
        await _cache.InvalidateAsync($"job:saved:user:{id.Value}:", ct);
        await _cache.InvalidateAsync($"job:all:user:{id.Value}:", ct);
        await _cache.InvalidateAsync($"job:mine:user:{id.Value}:", ct);
        await _cache.InvalidateAsync($"job:by-id:{request.Id}:", ct);
        return await Result.SuccessAsync(request.Id,"Job removed from saved jobs.");
    }
}
