
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities
{
    public class CandidatePreference : baseClass
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid CandidateProfileId { get; set; }

        public CandidateProfile CandidateProfile { get; set; } = null!;

        public decimal? MinimumMatchScore { get; set; }

        public string? PreferredLocation { get; set; }

        public bool RemoteOnly { get; set; }

        public int? MinimumYearsOfExperience { get; set; }

        public int? MaximumYearsOfExperience { get; set; }

        public bool NotificationEnabled { get; set; }
    }
}
