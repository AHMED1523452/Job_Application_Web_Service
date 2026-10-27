using JobApplication.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities{
    public class CandidateCVs
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        public string OriginalFileName { get; set; } = null!;

        public string StoragePath { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public long FileSize { get; set; }

        public CvProcesssingStatus ProcessingStatus { get; set; }

        public DateTime UploadedAtUtc { get; set; }

        public DateTime? ProcessedAtUtc { get; set; }

        public string? FileHash { get; set; }

        /// <summary>1:1 extraction record; null-safe consumers should check its Status, not assume text.</summary>
        public CandidateCvExtraction? Extraction { get; set; }

        public ICollection<JobApplications> Applications { get; set; }
            = new List<JobApplications>();

    }
}