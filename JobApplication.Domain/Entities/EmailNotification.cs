using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace JobApplication.Domain.Entities
{
    public class EmailNotification
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public Guid UserId { get; set; }


        public long? NotificationId { get; set; }

        public Notification? Notification { get; set; }

        public string RecipientEmail { get; set; } = null!;

        public string Subject { get; set; } = null!;

        public string Body { get; set; } = null!;

        public EmailStatus Status { get; set; }

        public DateTime? SentAtUtc { get; set; }

        public DateTime? FailedAtUtc { get; set; }

        public string? ErrorMessage { get; set; }

        public int RetryCount { get; set; }
    }
}
