using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities
{
    public class CandidateView
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        // FK -> CandidateProfile (OnDelete: Cascade)
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        // FK -> HrProfile (OnDelete: SetNull, nullable)
        public Guid? HrProfileId { get; set; }
        public HrProfile? HrProfile { get; set; }

        public DateTime ViewedAt { get; set; }
    }
}
