
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities
{
    public class Chat
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        // The chat is between one candidate and one recruiter.
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        public Guid HrProfileId { get; set; }
        public HrProfile HrProfile { get; set; } = null!;

        // Optional context for conversations that are tied to a specific application.
        public Guid? JobApplicationId { get; set; }
        public JobApplications? JobApplication { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }

        public ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
