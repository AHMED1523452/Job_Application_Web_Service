
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities
{
    public class Interview : baseClass
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        // FK -> JobApplication (OnDelete: Cascade)
        public Guid JobApplicationId { get; set; }
        public JobApplications JobApplication { get; set; } = null!;

        // FK -> HrProfile (interviewer) (OnDelete: SetNull, nullable)
        public Guid? InterviewerHrProfileId { get; set; }
        public HrProfile? InterviewerHrProfile { get; set; }

        public DateTime ScheduledAt { get; set; }
        public int DurationMinutes { get; set; }
        public InterviewType Type { get; set; } 
        public string? Location { get; set; }
        public InterviewStatus Status { get; set; }
        public string? Notes { get; set; }
    }
}
