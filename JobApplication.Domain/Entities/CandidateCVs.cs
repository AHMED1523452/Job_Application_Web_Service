using JobApplication.Domain.Enums;

namespace JobApplication.Domain.Entities{
    public class CandidateCVs{

    public Guid Id { get; set; }
    public Guid CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string StoragePath { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long FileSize { get; set; }

    public string? ExtractedText { get; set; }

    public CvProcesssingStatus ProcessingStatus { get; set; }

    public DateTime UploadedAtUtc { get; set; }

    public DateTime? ProcessedAtUtc { get; set; }

    public string? FileHash { get; set; }

    public ICollection<JobApplications> Applications { get; set; }
        = new List<JobApplications>();
    }
}