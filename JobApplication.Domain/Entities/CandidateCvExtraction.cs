using JobApplication.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities
{
    /// <summary>
    /// Holds the result of the background text-extraction process for one uploaded CV.
    /// Keeping extraction data in its own table means a pending or failed extraction can
    /// never surface as a silent null on the CV row itself.
    /// </summary>
    public class CandidateCvExtraction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid CandidateCvId { get; set; }
        public CandidateCVs CandidateCv { get; set; } = null!;

        public CvExtractionStatus Status { get; set; }

        public string? ExtractedText { get; set; }

        /// <summary>Reason the extraction failed, readable by consumers instead of a null reference.</summary>
        public string? ErrorMessage { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
    }
}
