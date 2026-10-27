namespace JobApplication.Application.Features.HrProfile.DTOs;

public sealed class HrProfileResponseDTO
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string CompanyName { get; set; } = null!;
    public string? CompanyDescription { get; set; }
    public string? CompanyWebsite { get; set; }
    public string? CompanyLogoPath { get; set; }
    public string? CompanyIndustry { get; set; }
    public string? CompanySize { get; set; }
    public string? CompanyLocation { get; set; }
    public string? CompanyType { get; set; }
}