using JobApplication.Application.Features.HrProfile.Commands;
using JobApplication.Application.Features.HrProfile.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using AutoMapper;
using HrProfileEntity = JobApplication.Domain.Entities.HrProfile;
using MediatR;

namespace JobApplication.Application.Features.HrProfile.Handlers;

public sealed class UpdateHrProfileHandler : IRequestHandler<UpdateHrProfileCommand, GenericResult<HrProfileResponseDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public UpdateHrProfileHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<GenericResult<HrProfileResponseDTO>> Handle(UpdateHrProfileCommand request, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserIdOrNull();
        if (!userId.HasValue)
            return await Result.FailureAsync<HrProfileResponseDTO>("Authentication is required.");

        // Load once with tracking for update (no unnecessary NoTracking load)
        var hrProfile = await _unitOfWork.Repository<HrProfileEntity>().GetByIdAsyncWithTracking(
            h => h.UserId == userId.Value,
            ct);

        if (hrProfile is null)
            return await Result.FailureAsync<HrProfileResponseDTO>("HR profile not found.");

        // Apply updates directly from request.Request to the tracked entity
        hrProfile.Name = request.Request.Name ?? hrProfile.Name;
        hrProfile.Address = request.Request.Address ?? hrProfile.Address;
        hrProfile.CompanyName = request.Request.CompanyName ?? hrProfile.CompanyName;
        hrProfile.CompanyDescription = request.Request.CompanyDescription ?? hrProfile.CompanyDescription;
        hrProfile.CompanyWebsite = request.Request.CompanyWebsite ?? hrProfile.CompanyWebsite;
        hrProfile.CompanyIndustry = request.Request.CompanyIndustry ?? hrProfile.CompanyIndustry;
        hrProfile.CompanySize = request.Request.CompanySize ?? hrProfile.CompanySize;
        hrProfile.CompanyLocation = request.Request.CompanyLocation ?? hrProfile.CompanyLocation;
        hrProfile.CompanyType = request.Request.CompanyType ?? hrProfile.CompanyType;

        hrProfile.UpdatedAt = DateTime.UtcNow;
        hrProfile.UpdatedBy = userId.Value.ToString();
        hrProfile.UpdatedByIp = _currentUser.UserIP();

        await _unitOfWork.Repository<HrProfileEntity>().EditAsync(hrProfile, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Return response using Select projection (no eager loading)
        var response = await _unitOfWork.Repository<HrProfileEntity>().GetByIdAsNoTrackingAsync<HrProfileResponseDTO>(
            h => h.Id == hrProfile.Id,
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

        if (response is null)
            return await Result.FailureAsync<HrProfileResponseDTO>("Failed to retrieve updated profile.");

        return await Result.SuccessAsync(response, "HR profile updated successfully.");
    }
}

