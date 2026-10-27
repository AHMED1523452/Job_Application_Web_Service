namespace JobApplication.Application.Features.HrProfile.DTOs;

public sealed class UpdateHrProfileRequestDTO
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyDescription { get; set; }
    public string? CompanyWebsite { get; set; }
    public string? CompanyIndustry { get; set; }
    public string? CompanySize { get; set; }
    public string? CompanyLocation { get; set; }
    public string? CompanyType { get; set; }
}