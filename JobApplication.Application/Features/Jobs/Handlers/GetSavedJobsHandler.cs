using AutoMapper;
using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Features.Jobs.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;

using JobEntity = JobApplication.Domain.Entities.Jobs;
namespace JobApplication.Application.Features.Jobs.Handlers;

public sealed class GetSavedJobsHandler : IRequestHandler<GetSavedJobsQuery, PaginatedResult<JobListItemsResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IApplicationCache _cache;

    public GetSavedJobsHandler(IUnitOfWork unitOfWork,
                               ICurrentUserService currentUser,
                               IMapper mapper,
                               IApplicationCache cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<PaginatedResult<JobListItemsResponseDTO>> Handle(GetSavedJobsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return new PaginatedResult<JobListItemsResponseDTO> { IsSuccess = false, Message = "Authentication is required." };

        Guid candidateProfileId = await _unitOfWork.Repository<CandidateProfile>().GetByIdAsyncWithTracking<Guid>(
            p => p.UserId == userId.Value && 
                !p.IsDeleted && !p.IsArchived && 
                 p.IsActive,
            selector: op => op.Id,
            ct);

        if (candidateProfileId == default)
            return new PaginatedResult<JobListItemsResponseDTO> { IsSuccess = false, Message = "Candidate profile not found." };

        //. needing for enumerator 
        IEnumerable<Guid> savedJobIds = await _unitOfWork.Repository<SavedJobs>()
            .GetAllAsync<Guid>(s => s.CandidateProfileId == candidateProfileId, 
                           selector: sel => sel.JobId, ct);

        var (jobs, total) = await _unitOfWork.Repository<JobEntity>().GetPagedAsync<JobEntity>(
            request.Page,
            request.PageSize,
            x => savedJobIds.ToList().Contains(x.Id) && !x.IsDeleted && !x.IsArchived,
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
        var cacheKey = $"job:saved:user:{userId.Value}:page:{request.Page}:pageSize:{request.PageSize}";
        await _cache.CreateAsync(cacheKey, response, ct);
        return response;
    }
}