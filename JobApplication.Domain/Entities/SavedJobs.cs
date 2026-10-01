using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Entities
{
    public class SavedJobs
    {
       public Guid Id { get; set; }

        // FK -> CandidateProfile (OnDelete: Cascade)
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        // FK -> Job (OnDelete: Cascade)
        public Guid JobId { get; set; }
        public Jobs Job { get; set; } = null!;

        public DateTime SavedAt { get; set; }
        public string? Notes { get; set; }
    }
}
