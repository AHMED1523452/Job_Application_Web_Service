using AutoMapper;
using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Features.Jobs.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Enums;
using MediatR;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

using HrProfileEntity = JobApplication.Domain.Entities.HrProfile;
using JobEntity = JobApplication.Domain.Entities.Jobs;
namespace JobApplication.Application.Features.Jobs.Handlers;

public sealed class GetJobsHandler : IRequestHandler<GetJobsQuery, PaginatedResult<JobListItemsResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IApplicationCache _cache;

    public GetJobsHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper, IApplicationCache cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<PaginatedResult<JobListItemsResponseDTO>> Handle(GetJobsQuery request, CancellationToken ct)
    {
        var f = request.Request;
        var now = DateTime.UtcNow;
        var keyword = string.IsNullOrWhiteSpace(f.Keyword) ? null : f.Keyword.Trim();
        var userKey = _currentUser.GetCurrentUserIdOrNull()?.ToString() ?? "anonymous";
        var skillKey = f.SkillIds is null ? string.Empty : string.Join(',', f.SkillIds.OrderBy(id => id));

        var cacheKey = $"job:all:user:{userKey}:page:{request.Page}:pageSize:{request.PageSize}" +
                       $" :keyword:{Uri.EscapeDataString(keyword ?? string.Empty)}:category:{f.JobCategoryId}" +
                       $":employment:{f.EmploymentType}:location:{Uri.EscapeDataString(f.Location?.Trim().ToLowerInvariant() ?? string.Empty)}" +
                       $":skills:{skillKey}:lat:{f.Latitude}:lon:{f.Longitude}:radius:{f.RadiusKm}:sort:{f.SortBy}";

        var cached = await _cache.GetAsync<PaginatedResult<JobListItemsResponseDTO>>(cacheKey, ct);
        if (cached is not null)
            return cached;

        var searchPoint = f.Latitude.HasValue && f.Longitude.HasValue
            ? new Point(f.Longitude.Value, f.Latitude.Value) { SRID = 4326 }
            : null;
        var radiusMeters = f.RadiusKm.GetValueOrDefault() * 1000;

        var userId = _currentUser.GetCurrentUserIdOrNull();
        
        // Get recruiter first if user is authenticated
        Guid? recruiterId = null;
        if (userId.HasValue)
        {
            var recruiter = await _unitOfWork.Repository<HrProfileEntity>().GetByIdAsyncWithTracking(x => x.UserId == userId.Value  && 
                                                                                                    x.IsActive && 
                                                                                                    !x.IsDeleted &&
                                                                                                    !x.IsArchived, ct);
            if (recruiter != null)
                recruiterId = recruiter.Id; //. to be assigned must be bringing the item with tracking to do any process
        }

        //. predicate filteration and search 
        Expression<Func<JobEntity, bool>> predicate = job =>
            !job.IsDeleted &&
            !job.IsArchived &&
             job.jobStatus == JobStatus.Published
            && (!job.ApplicationDeadlineUtc.HasValue || job.ApplicationDeadlineUtc > now)
            && (!f.JobCategoryId.HasValue || job.JobCategoryId == f.JobCategoryId.Value)
            && (!f.EmploymentType.HasValue || job.employmentType == f.EmploymentType.Value)
            && (keyword == null || job.Title.Contains(keyword) || (job.Description != null && job.Description.Contains(keyword)))
            && (string.IsNullOrWhiteSpace(f.Location) || job.Location.Contains(f.Location) || (job.Address != null && job.Address.Contains(f.Location)))
            && (f.SkillIds == null || f.SkillIds.Count == 0 || job.JobRequirements.Any(r => r.SkillId.HasValue && f.SkillIds.Contains(r.SkillId.Value)))
            && (searchPoint == null || (job.LocationPoint != null && job.LocationPoint.Distance(searchPoint) <= radiusMeters))
            && (!userId.HasValue || recruiterId.HasValue && job.HrProfileId == recruiterId.Value);

        var (jobs, total) = await _unitOfWork.Repository<JobEntity>().GetPagedAsync<JobEntity>(
            request.Page,
            request.PageSize,
            predicate,
            x => x,
            nameof(JobEntity.CreatedAt),
            true,
            null,   // searchTerm
            ct);    // cancellationToken

        var items = await JobHandlerHelpers.ToListAsync(_unitOfWork, _mapper, jobs, userId, ct);

        var response = new PaginatedResult<JobListItemsResponseDTO>
        {
            Items = items,
            Pagination = new PaginationMetadata
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
            }
        };
        await _cache.CreateAsync(cacheKey, response, ct);
        return response;
    }
}
