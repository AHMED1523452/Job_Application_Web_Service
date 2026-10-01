
namespace JobApplication.Domain.Entities
{
    public class Message  
    {
         public Guid Id { get; set; }

        // FK -> Chat (OnDelete: Cascade)
        public Guid ChatId { get; set; }
        public Chat Chat { get; set; } = null!;

        // FK -> ApplicationUser (sender) (OnDelete: SetNull, nullable)
        public Guid? SenderId { get; set; }

        public string Content { get; set; } = null!;

        // FK -> MediaFile (attachment) (OnDelete: SetNull, nullable)
        public Guid? MediaFileId { get; set; }
        public MediaFile? MediaFile { get; set; }

        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime EditedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }

    }
}
