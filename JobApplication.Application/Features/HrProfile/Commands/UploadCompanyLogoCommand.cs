using JobApplication.Application.Features.HrProfile.DTOs;
using JobApplication.Application.Patterns;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace JobApplication.Application.Features.HrProfile.Commands;

public sealed record UploadCompanyLogoCommand(IFormFile LogoFile)
    : IRequest<GenericResult<UploadCompanyLogoResponseDTO>>;