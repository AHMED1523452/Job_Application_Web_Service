using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace JobApplication.Domain.Entities
{
    public class CandidateRecommendation : baseClass
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid JobId { get; set; }

        public Jobs Job { get; set; } = null!;

        public Guid CandidateProfileId { get; set; }

        public CandidateProfile CandidateProfile { get; set; } = null!;

        public decimal MatchScore { get; set; }

        public string? Reason { get; set; }

        public bool IsViewed { get; set; }

        public bool IsDismissed { get; set; }
    }
}
