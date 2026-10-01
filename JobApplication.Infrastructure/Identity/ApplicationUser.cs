using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace JobApplication.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        [Required(ErrorMessage = "Full Name field is required. "), MaxLength(100,
                                        ErrorMessage = "less than 100 characters")]
        public string FullName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsRevoked { get; set; } = false;
    }
}
