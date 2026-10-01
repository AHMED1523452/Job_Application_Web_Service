using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities
{
    public class VerificationCode
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        [Required(ErrorMessage =  "Email field is required. "), ]
        public string Email { get; set; }

        public string CodeHash { get; set; } = null!;

        public VerificationCodePurpose Purpose { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }

        public DateTime? UsedAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
