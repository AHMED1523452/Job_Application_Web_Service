using JobApplication.Application.Features.HrProfile.DTOs;
using JobApplication.Application.Features.HrProfile.Queries;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using HrProfileEntity = JobApplication.Domain.Entities.HrProfile;
using MediatR;

namespace JobApplication.Application.Features.HrProfile.Handlers;

public sealed class GetHrProfileHandler : IRequestHandler<GetHrProfileQuery, GenericResult<HrProfileResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetHrProfileHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<GenericResult<HrProfileResponseDTO>> Handle(GetHrProfileQuery request, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<HrProfileResponseDTO>("Authentication is required.");

        // Read using NoTracking + Select projection (no eager loading of entire Jobs table)
        var hrProfile = await _unitOfWork.Repository<HrProfileEntity>().GetByIdAsNoTrackingAsync<HrProfileResponseDTO>(
            h => h.UserId == userId.Value,
            h => new HrProfileResponseDTO
            {
                Id = h.Id,
                UserId = h.UserId,
                Name = h.Name,
                Address = h.Address,
                CompanyName = h.CompanyName,
                CompanyDescription = h.CompanyDescription,
                CompanyWebsite = h.CompanyWebsite,
                CompanyLogoPath = h.CompanyLogoPath,
                CompanyIndustry = h.CompanyIndustry,
                CompanySize = h.CompanySize,
                CompanyLocation = h.CompanyLocation,
                CompanyType = h.CompanyType
            },
            ct);

        if (hrProfile is null)
            return await Result.FailureAsync<HrProfileResponseDTO>("HR profile not found.");

        return await Result.SuccessAsync(hrProfile, "HR profile retrieved successfully.");
    }
}