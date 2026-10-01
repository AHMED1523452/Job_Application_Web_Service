using JobApplication.Application.Features.JobApplication.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.JobApplication.Commands;

public sealed record ChangeJobApplicationStatusCommand(Guid ApplicationId, ChangeJobApplicationStatusRequestDTO Request)
    : IRequest<GenericResult<CloseJobApplicationResponseDTO>>;
