
namespace JobApplication.Application.Features.Candidate.DTOs
{
    public sealed class CandidateSkillResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public Guid SkillId { get; set; }
        public string? SkillName { get; set; }
        public SkillProficiencyLevel ProficiencyLevel { get; set; }
        public int? YearsOfExperience { get; set; }
    }

}
