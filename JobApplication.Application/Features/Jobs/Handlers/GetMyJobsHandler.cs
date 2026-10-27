using AutoMapper;
using FluentValidation;
using JobApplication.Application.Features.Jobs.DTOs;
using JobApplication.Application.Features.Jobs.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;
using System.Linq.Expressions;

using HrProfileEntity = JobApplication.Domain.Entities.HrProfile;
using JobEntity = JobApplication.Domain.Entities.Jobs;
namespace JobApplication.Application.Features.Jobs.Handlers;

public sealed class GetMyJobsHandler : IRequestHandler<GetMyJobsQuery, PaginatedResult<JobListItemsResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IApplicationCache _cache;

    public GetMyJobsHandler(IUnitOfWork unitOfWork,
                            ICurrentUserService currentUser,
                            IMapper mapper,
                            IApplicationCache cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<PaginatedResult<JobListItemsResponseDTO>> Handle(GetMyJobsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return new PaginatedResult<JobListItemsResponseDTO> { IsSuccess = false, Message = "Authentication is required." };

        var cacheKey = $"job:mine:user:{userId.Value}:page:{request.Page}:pageSize:{request.PageSize}";

        var cached = await _cache.GetAsync<PaginatedResult<JobListItemsResponseDTO>>(cacheKey, ct);
        if (cached is not null)
            return cached;

        var recruiter = await _unitOfWork.Repository<HrProfileEntity>().GetByIdAsync(x => x.UserId == userId.Value, ct);

        if (recruiter is null)
            return new PaginatedResult<JobListItemsResponseDTO> { IsSuccess = false, Message = "Recruiter profile not found." };

        var (jobs, total) = await _unitOfWork.Repository<JobEntity>().GetPagedAsync<JobEntity>(
            request.Page,
            request.PageSize,
            x => x.HrProfileId == recruiter.Id && !x.IsDeleted && !x.IsArchived,
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

public class GetMyJobsQueryValidator : AbstractValidator<GetMyJobsQuery>
{
    public GetMyJobsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}