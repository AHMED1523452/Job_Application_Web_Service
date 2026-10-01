using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace JobApplication.Domain.Entities
{
    public class SMSNotification : baseClass 
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        // FK -> ApplicationUser (OnDelete: SetNull -> make nullable)
        public Guid? UserId { get; set; }

        public string PhoneNumber { get; set; } = null!;
        public string Message { get; set; } = null!;
        public SMSNotificationStatus Status { get; set; } 
        public string? Provider { get; set; }
        public string? ProviderMessageId { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
