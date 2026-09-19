using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace JobApplication.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        [Required(ErrorMessage = "Full Name is required"),
            MaxLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
        public string FullName { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;//. will be used in updating and soft deleting 
    }
}
