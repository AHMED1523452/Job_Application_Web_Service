using JobApplication.Application.Features.Candidate.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Candidate.Commands;

public sealed record UploadCandidateCvCommand(UploadCandidateCvRequestDTO Request)
    : IRequest<GenericResult<CandidateCvResponseDTO>>;
