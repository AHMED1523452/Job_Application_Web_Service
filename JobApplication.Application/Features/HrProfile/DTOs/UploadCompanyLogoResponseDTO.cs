namespace JobApplication.Application.Features.HrProfile.DTOs;

public sealed class UploadCompanyLogoResponseDTO
{
    public string? CompanyLogoPath { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsAlreadyExisting { get; set; }
}