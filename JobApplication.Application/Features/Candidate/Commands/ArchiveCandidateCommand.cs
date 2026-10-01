using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.Candidate.Commands;

public sealed record ArchiveCandidateCommand : IRequest<GenericResult<Guid>>;
