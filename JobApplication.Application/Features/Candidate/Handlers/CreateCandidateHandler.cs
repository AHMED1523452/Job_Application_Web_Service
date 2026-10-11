using AutoMapper;
using JobApplication.Application.Features.Candidate.Commands;
using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Candidate.Handlers;

public sealed class CreateCandidateHandler : IRequestHandler<CreateCandidateCommand, GenericResult<CandidateResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IApplicationCache _cache;

    public CreateCandidateHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper, IApplicationCache cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<GenericResult<CandidateResponseDTO>> Handle(CreateCandidateCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<CandidateResponseDTO>("Authentication is required.");

        var candidates = _unitOfWork.Repository<Domain.Entities.CandidateProfile>();

        //. if the user already has a candidate profile, return an error
        if (await candidates.AnyAsync(candidate => candidate.UserId == userId.Value, cancellationToken))
            return await Result.FailureAsync<CandidateResponseDTO>("A candidate profile already exists for this account.");
         
        //. Automapper more fast (to the model) 
        //. Map the request DTO to the CandidateProfile entity
        var candidate = _mapper.Map<Domain.Entities.CandidateProfile>(command.Request);
        candidate.Skills = _mapper.Map<List<CandidateSkill>>(command.Request.Skills);
        candidate.experiences = _mapper.Map<List<CandidateExperience>>(command.Request.Experiences);
        candidate.educations = _mapper.Map<List<CandidateEducation>>(command.Request.Educations);
        candidate.certifications = _mapper.Map<List<CandidateCertification>>(command.Request.Certifications);

        candidate.Preference = command.Request.Preference is null
            ? null
            : _mapper.Map<CandidatePreference>(command.Request.Preference);

        candidate.Id = Guid.NewGuid();
        candidate.UserId = userId.Value;
        candidate.IsActive = true;
        candidate.CreatedAt = DateTime.UtcNow;
        candidate.UpdatedAt = candidate.CreatedAt;
        candidate.CreatedBy = userId.Value.ToString();
        candidate.UpdatedBy = candidate.CreatedBy;
        candidate.CreatedByIp = _currentUser.UserIP();

        CandidateProfileInitializer.InitializeChildren(candidate, candidate.CreatedBy);
        await candidates.AddAsync(candidate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
      
        //.removing the old keys list for candidates and setting the new candidates table or list
        await _cache.InvalidateAsync("candidate:", cancellationToken);
        //. removing the whole job application or the job application
        //.          that related with the old candidates and setting new candidates
        await _cache.InvalidateAsync("job-application:", cancellationToken);

        return await Result.SuccessAsync(
            await CandidateResponseBuilder.BuildAsync(candidate, _unitOfWork, _mapper, cancellationToken),
            "Candidate profile created successfully.");
    }

}
