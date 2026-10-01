
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities
{
    public class CandidateExperience : baseClass
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        // FK -> CandidateProfile (OnDelete: Cascade)
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        public string JobTitle { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string? Location { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string? Description { get; set; }
    }
}
