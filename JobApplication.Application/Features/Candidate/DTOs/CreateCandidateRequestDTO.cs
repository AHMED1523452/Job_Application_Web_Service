using Microsoft.AspNetCore.Http;

namespace JobApplication.Application.Features.Candidate.DTOs;

public sealed class CreateCandidateRequestDTO
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? Location { get; set; }
    public IFormFile? profileImage { get; set; }
    public List<CandidateSkillRequestDTO> Skills { get; set; } = new();
    public List<CreateCandidateExperienceRequestDTO> Experiences { get; set; } = new();
    public List<CreateCandidateEducationRequestDTO> Educations { get; set; } = new();
    public List<CreateCandidateCertificationRequestDTO> Certifications { get; set; } = new();
    public CandidatePreferenceRequestDTO? Preference { get; set; }
}
