using AutoMapper ;
using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Features.Candidate.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;
using NetTopologySuite.Index.HPRtree;

namespace JobApplication.Application.Features.Candidate.Handlers;

public sealed class GetMyCandidateHandler : IRequestHandler<GetMyCandidateQuery, GenericResult<CandidateResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IApplicationCache _cache;

    public GetMyCandidateHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper, IApplicationCache cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<GenericResult<CandidateResponseDTO>> Handle(GetMyCandidateQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<CandidateResponseDTO>("Authentication is required.");

        var cacheKey = $"candidate:mine:user:{userId.Value}";
        var cached = await _cache.GetAsync<GenericResult<CandidateResponseDTO>>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        var candidateResponse = await _unitOfWork.Repository<Domain.Entities.CandidateProfile>()
            .GetByIdAsNoTrackingAsync(
                candidate => candidate.UserId == userId.Value
                    && !candidate.IsDeleted
                    && !candidate.IsArchived,
                CandidateProfileResponseSelector.Select,
                cancellationToken);

        if (candidateResponse is null)
            return await Result.FailureAsync<CandidateResponseDTO>("Create a candidate profile before requesting your profile.");

        var candidateProfile = _mapper.Map<Domain.Entities.CandidateProfile>(candidateResponse);
        var response = await CandidateResponseBuilder.BuildAsync(
            candidateProfile,
            _unitOfWork,
            _mapper,
            cancellationToken);

        var result = await Result.SuccessAsync(response);
        await _cache.CreateAsync(cacheKey, result, cancellationToken);
        return result;
    }
}
