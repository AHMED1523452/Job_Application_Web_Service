using JobApplication.Domain.Enums;

namespace JobApplication.Application.Features.Candidate.DTOs;

public sealed class CandidateSkillRequestDTO
{
    public Guid SkillId { get; set; }
    public SkillProficiencyLevel ProficiencyLevel { get; set; }
    public int? YearsOfExperience { get; set; }
}
