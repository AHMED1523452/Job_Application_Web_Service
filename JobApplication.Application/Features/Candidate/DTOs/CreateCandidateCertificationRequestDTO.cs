namespace JobApplication.Application.Features.Candidate.DTOs;

public sealed class CreateCandidateCertificationRequestDTO
{
    public string Name { get; set; } = string.Empty;
    public string IssuingOrganization { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? CredentialId { get; set; }
    public string? CredentialUrl { get; set; }
}
