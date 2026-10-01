using AutoMapper;
using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Features.Jobs.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Enums;
using MediatR;
using NetTopologySuite.Geometries;

using JobEntity = JobApplication.Domain.Entities.Jobs;
namespace JobApplication.Application.Features.Jobs.Handlers;

public sealed class GetJobsHandler : IRequestHandler<GetJobsQuery, PaginatedResult<JobListItemsResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    public GetJobsHandler(IUnitOfWork unitOfWork,
                          ICurrentUserService currentUser,
                          IMapper mapper)
    { 
        _unitOfWork = unitOfWork; 
        _currentUser = currentUser;
        _mapper = mapper; 
    }

    public async Task<PaginatedResult<JobListItemsResponseDTO>> Handle(GetJobsQuery request, CancellationToken ct)
    {
        var f = request.Request; var now = DateTime.UtcNow;
        var keyword = string.IsNullOrWhiteSpace(f.Keyword) ? null : f.Keyword.Trim();
        //. Validating if the point is existing or not if not we will create another point 
        var searchPoint = f.Latitude.HasValue && f.Longitude.HasValue
            ? new Point(f.Longitude.Value, f.Latitude.Value) { SRID = 4326 }
            : null;

        var radiusMeters = f.RadiusKm.GetValueOrDefault() * 1000;

        //. Predicate for the filteration of the Generic Repository  
        var predicate = (System.Linq.Expressions.Expression<Func<JobEntity, bool>>)(job =>
            !job.IsDeleted && !job.IsArchived && job.jobStatus == JobStatus.Published
            && (!job.ApplicationDeadlineUtc.HasValue || job.ApplicationDeadlineUtc > now)
            && (!f.JobCategoryId.HasValue || job.JobCategoryId == f.JobCategoryId.Value)
            && (!f.EmploymentType.HasValue || job.employmentType == f.EmploymentType.Value)
            && (keyword == null || job.Title.Contains(keyword) || (job.Description != null && job.Description.Contains(keyword)))
            && (string.IsNullOrWhiteSpace(f.Location) || job.Location.Contains(f.Location) || (job.Address != null && job.Address.Contains(f.Location)))
            && (f.SkillIds == null || f.SkillIds.Count == 0 || job.JobRequirements.Any(r => r.SkillId.HasValue && f.SkillIds.Contains(r.SkillId.Value)))
            && (searchPoint == null || (job.LocationPoint != null && job.LocationPoint.Distance(searchPoint) <= radiusMeters)));

        IEnumerable<JobEntity> entities;
        int total;
        if (f.SortBy == "relevance" && keyword is not null)
        {
            var matchingJobs = await _unitOfWork.Repository<JobEntity>().GetAllAsync<JobEntity>(predicate,
                null, ct, x => x.HrProfile, x => x.JobCategory, x => x.JobRequirements);

            var ranked = matchingJobs.OrderByDescending(job => string.Equals(job.Title, keyword, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(job => job.Title.StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(job => job.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(job => job.CreatedAt).ToList();
            total = ranked.Count;
            entities = ranked.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();
        }
        else
        {
            var page = await _unitOfWork.Repository<JobEntity>().GetPagedAsync<JobEntity>(request.Page, request.PageSize,
                predicate, x => x, nameof(JobEntity.CreatedAt), true, null, ct,
                x => x.HrProfile, x => x.JobCategory, x => x.JobRequirements);
            entities = page.Items;
            total = page.TotalCount;
        }
        var items = await JobHandlerHelpers.ToListAsync(_unitOfWork, _mapper, entities, _currentUser.GetCurrentUserIdOrNull(), ct);
        if (searchPoint is not null)
        {
            foreach (var (item, entity) in items.Zip(entities))
                item.DistanceKm = entity.LocationPoint?.Distance(searchPoint) / 1000.0;
        }
        return new PaginatedResult<JobListItemsResponseDTO> { 
            Items = items,
            Pagination = new PaginationMetadata 
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)request.PageSize) 
            } 
        };
    }
}
