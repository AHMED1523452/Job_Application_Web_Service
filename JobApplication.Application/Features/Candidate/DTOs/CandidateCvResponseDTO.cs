

using JobApplication.Domain.Enums;

namespace JobApplication.Application.Features.Candidate.DTOs
{
    public class CandidateCvResponseDTO
    {
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public CvProcesssingStatus ProcessingStatus { get; set; }
        public DateTime UploadedAtUtc { get; set; }
        public DateTime? ProcessedAtUtc { get; set; }
    }
}
