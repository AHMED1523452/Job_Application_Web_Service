
using JobApplication.Domain.Enums;

namespace JobApplication.Domain.Entities
{
    public class Notification : baseClass
    {
        public long Id { get; set; }
        public Guid UserId { get; set; }

        public NotificationType Type { get; set; }

        public string Title { get; set; } = null!;

        public string Message { get; set; } = null!;

        public bool IsRead { get; set; }

        public DateTime? ReadAtUtc { get; set; }
    }
}
