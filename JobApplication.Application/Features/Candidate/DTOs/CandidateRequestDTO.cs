using JobApplication.Domain.Enums;

namespace JobApplication.Application.Features.Candidate.DTOs;

public sealed class CandidateRequestDTO
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? Location { get; set; }
    public Guid? ProfileImageId { get; set; }
    public List<CandidateSkillRequestDTO> Skills { get; set; } = new();
    public List<CandidateExperienceRequestDTO> Experiences { get; set; } = new();
    public List<CandidateEducationRequestDTO> Educations { get; set; } = new();
    public List<CandidateCertificationRequestDTO> Certifications { get; set; } = new();
    public CandidatePreferenceRequestDTO? Preference { get; set; }
}

public sealed class CandidateSkillRequestDTO
{
    public Guid SkillId { get; set; }
    public SkillProficiencyLevel ProficiencyLevel { get; set; }
    public int? YearsOfExperience { get; set; }
}

public sealed class CandidateExperienceRequestDTO
{
    public Guid? Id { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string? Description { get; set; }
}

public sealed class CandidateEducationRequestDTO
{
    public Guid? Id { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string? FieldOfStudy { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? GradeOrGpa { get; set; }
    public string? Description { get; set; }
}

public sealed class CandidateCertificationRequestDTO
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IssuingOrganization { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? CredentialId { get; set; }
    public string? CredentialUrl { get; set; }
}

public sealed class CandidatePreferenceRequestDTO
{
    public decimal? MinimumMatchScore { get; set; }
    public string? PreferredLocation { get; set; }
    public bool RemoteOnly { get; set; }
    public int? MinimumYearsOfExperience { get; set; }
    public int? MaximumYearsOfExperience { get; set; }
    public bool NotificationEnabled { get; set; }
}
