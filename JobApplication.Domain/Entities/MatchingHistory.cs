
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities
{
    public class MatchingHistory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        // FK -> CandidateProfile (OnDelete: Cascade)
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        // FK -> Job (OnDelete: Cascade)
        public Guid JobId { get; set; }
        public Jobs Job { get; set; } = null!;

        public double MatchScore { get; set; }
        public string AlgorithmVersion { get; set; } = null!;
        public string Status { get; set; } = null!; // enum: Suggested/Viewed/Applied/Dismissed
        public DateTime MatchedAt { get; set; }
    }
}
