using AutoMapper;
using FluentValidation;
using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Features.Candidate.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;
using System.Linq.Expressions;

namespace JobApplication.Application.Features.Candidate.Handlers;

public sealed class GetCandidatesHandler : IRequestHandler<GetCandidatesQuery, GenericResult<PaginatedResult<CandidateResponseDTO>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IApplicationCache _cache;

    public GetCandidatesHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IApplicationCache cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<GenericResult<PaginatedResult<CandidateResponseDTO>>> Handle(GetCandidatesQuery query, CancellationToken cancellationToken)
    {
        var searchKey = Uri.EscapeDataString(query.Search?.Trim().ToLowerInvariant() ?? string.Empty);
        var userId = _currentUser.GetCurrentUserIdOrNull()?.ToString() ?? "anonymous";

        var cacheKey = $"candidate:all:user:{userId}:page:{query.Page}:pageSize:{query.PageSize}:search:{searchKey}:sort:{query.OrderBy}:desc:{query.OrderDesc}";
        var cached = await _cache.GetAsync<GenericResult<PaginatedResult<CandidateResponseDTO>>>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        Expression<Func<Domain.Entities.CandidateProfile, bool>>? predicate = null;
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            predicate = candidate => !candidate.IsDeleted && !candidate.IsArchived && candidate.IsActive
                && ((candidate.Title != null && candidate.Title.Contains(search))
                    || (candidate.Description != null && candidate.Description.Contains(search))
                    || (candidate.Location != null && candidate.Location.Contains(search)));
        }
        else
        {
            predicate = candidate => !candidate.IsDeleted && !candidate.IsArchived && candidate.IsActive;
        }

        var page = await _unitOfWork.Repository<Domain.Entities.CandidateProfile>().GetPagedAsync<Domain.Entities.CandidateProfile>(
            query.Page, query.PageSize, predicate, candidate => candidate,
            query.OrderBy,
            query.OrderDesc,
            null,
            cancellationToken,
            candidate => candidate.Skills,
            candidate => candidate.experiences,
            candidate => candidate.educations,
            candidate => candidate.certifications,
            candidate => candidate.CVs,
            candidate => candidate.Preference);

        var items = await CandidateResponseBuilder.BuildManyAsync(page.Items, _unitOfWork, _mapper, cancellationToken);
        var result = new PaginatedResult<CandidateResponseDTO>
        {
            Items = items,
            Pagination = new PaginationMetadata
            {
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = page.TotalCount,
                TotalPages = (int)Math.Ceiling(page.TotalCount / (double)query.PageSize)
            }
        };

        var response = await Result.SuccessAsync(result, "Candidates retrieved successfully.");
        await _cache.CreateAsync(cacheKey, response, cancellationToken);
        return response;
    }
}
