using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Features.Candidate.DTOs;

public sealed class CandidateResponseDTO
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ProfileImageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? Location { get; set; }

    //. Preference model
    public CandidatePreferenceResponseDTO? preference { get; set; }

    public List<CandidateSkillResponseDTO> Skills { get; set; } = new();
    public List<CandidateExperienceResponseDTO> Experiences { get; set; } = new();
    public List<CandidateEducationResponseDTO> Educations { get; set; } = new();
    public List<CandidateCertificationResponseDTO> Certifications { get; set; } = new();
    public List<CandidateCvResponseDTO> CVs { get; set; } = new();
}
