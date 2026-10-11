using AutoMapper;
using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Features.Candidate.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Candidate.Handlers;

public sealed class GetCandidateByIdHandler : IRequestHandler<GetCandidateByIdQuery, GenericResult<CandidateResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IApplicationCache _cache;

    public GetCandidateByIdHandler(IUnitOfWork unitOfWork, IMapper mapper, IApplicationCache cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<GenericResult<CandidateResponseDTO>> Handle(
        GetCandidateByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"candidate:by-id:{request.Id}";
        var cached = await _cache.GetAsync<GenericResult<CandidateResponseDTO>>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        var candidateResponse = await _unitOfWork.Repository<Domain.Entities.CandidateProfile>()
            .GetByIdAsNoTrackingAsync(
                candidate => candidate.Id == request.Id
                    && !candidate.IsDeleted
                    && !candidate.IsArchived
                    && candidate.IsActive,
                CandidateProfileResponseSelector.Select,
                cancellationToken);

        if (candidateResponse is null)
            return await Result.FailureAsync<CandidateResponseDTO>("Candidate profile not found.");

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
