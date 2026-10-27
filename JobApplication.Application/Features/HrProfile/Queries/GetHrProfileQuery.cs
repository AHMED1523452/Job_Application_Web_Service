using JobApplication.Application.Features.HrProfile.DTOs;
using JobApplication.Application.Patterns;
using MediatR;

namespace JobApplication.Application.Features.HrProfile.Queries;

public sealed record GetHrProfileQuery : IRequest<GenericResult<HrProfileResponseDTO>>;