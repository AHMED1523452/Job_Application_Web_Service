using JobApplication.Application.Features.HrProfile.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.HrProfile.Commands;

public sealed record UpdateHrProfileCommand(UpdateHrProfileRequestDTO Request)
    : IRequest<GenericResult<HrProfileResponseDTO>>;