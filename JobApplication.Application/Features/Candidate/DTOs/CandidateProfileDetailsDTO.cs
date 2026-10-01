
namespace JobApplication.Application.Features.Candidate.DTOs
{
    public class CandidateProfileDetailsDTO
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public int? YearsOfExperience { get; init; }
        public string? Location { get; init; }
        public Guid? ProfileImageId { get; init; }

        public List<CandidateSkillResponseDTO> Skills { get; init; } = [];
        public List<CandidateExperienceResponseDTO> Experiences { get; init; } = [];
        public List<CandidateEducationResponseDTO> Educations { get; init; } = [];
        public List<CandidateCertificationResponseDTO> Certifications { get; init; } = [];
        public List<CandidateCvResponseDTO> CVs { get; init; } = [];
        public CandidatePreferenceResponseDTO? Preference { get; init; }
    }
}
