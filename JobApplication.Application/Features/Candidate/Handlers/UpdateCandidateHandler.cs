using AutoMapper;
using JobApplication.Application.Features.Candidate.Commands;
using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Candidate.Handlers;

public sealed class UpdateCandidateHandler : IRequestHandler<UpdateCandidateCommand, GenericResult<CandidateResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public UpdateCandidateHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<GenericResult<CandidateResponseDTO>> Handle(UpdateCandidateCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<CandidateResponseDTO>("Authentication is required.");

        var repository = _unitOfWork.Repository<Domain.Entities.CandidateProfile>();
        var candidate = await repository.GetByIdAsyncWithTracking(
            item => item.Id == command.Id && !item.IsDeleted && !item.IsArchived,
            cancellationToken,
            item => item.Skills,
            item => item.experiences,
            item => item.educations,
            item => item.certifications,
            item => item.Preference);

        if (candidate is null)
            return await Result.FailureAsync<CandidateResponseDTO>("Candidate profile not found.");

        if (candidate.UserId != userId.Value)
            return await Result.FailureAsync<CandidateResponseDTO>("You can only update your own candidate profile.");

        if (!CandidateProfileCollectionUpdater.RequestReferencesOwnedChildren(candidate, command.Request))
            return await Result.FailureAsync<CandidateResponseDTO>("A supplied experience, education, or certification ID does not belong to this profile.");

        //. update not create 
        _mapper.Map(command.Request, candidate);
        candidate.UpdatedAt = DateTime.UtcNow;
        candidate.UpdatedBy = userId.Value.ToString();
        candidate.UpdatedByIp = _currentUser.UserIP();

        await CandidateProfileCollectionUpdater.ApplyUpdateAsync(
            _unitOfWork, _mapper, candidate, command.Request, candidate.UpdatedBy, cancellationToken);

        await repository.EditAsync(candidate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await Result.SuccessAsync(
            await CandidateResponseBuilder.BuildAsync(candidate, _unitOfWork, _mapper, cancellationToken),
            "Candidate profile updated successfully.");
    }
}
