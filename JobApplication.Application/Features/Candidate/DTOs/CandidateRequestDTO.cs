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
