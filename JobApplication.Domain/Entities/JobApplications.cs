using JobApplication.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities
{
    public class JobApplications : baseClass
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid JobId { get; set; }
        public Jobs Job { get; set; } = null!;

        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        public Guid CandidateCvId { get; set; }
        public CandidateCVs CandidateCv { get; set; } = null!;

        public JobApplicationStatus Status { get; set; }

        public DateTime AppliedAtUtc { get; set; }

        public DateTime? ReviewedAtUtc { get; set; }

        public string? RejectionReason { get; set; }
        public string? CoverLetter { get; set; }

        public AtsAnalysis? AtsAnalysis { get; set; } //. For relation 1-1

        public ICollection<Chat> Chats { get; set; } = new List<Chat>();
    }
}
