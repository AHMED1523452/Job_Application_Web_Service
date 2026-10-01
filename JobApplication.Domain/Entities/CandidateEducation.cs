
namespace JobApplication.Domain.Entities
{
    public class CandidateEducation : baseClass
    {
        public Guid Id { get; set; }

        // FK -> CandidateProfile (OnDelete: Cascade)
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        public string InstitutionName { get; set; } = null!;
        public string Degree { get; set; } = null!;
        public string? FieldOfStudy { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? GradeOrGpa { get; set; }
        public string? Description { get; set; }
    }
}
