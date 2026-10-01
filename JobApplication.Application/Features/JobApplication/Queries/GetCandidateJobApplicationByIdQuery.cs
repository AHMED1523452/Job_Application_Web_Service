using JobApplication.Application.Features.JobApplication.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.JobApplication.Queries;

public sealed record GetCandidateJobApplicationByIdQuery(Guid CandidateProfileId, Guid ApplicationId)
    : IRequest<GenericResult<JobApplicationResponseDTO>>;
