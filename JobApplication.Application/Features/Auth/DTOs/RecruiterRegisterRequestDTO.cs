using System.ComponentModel.DataAnnotations;

namespace JobApplication.Application.Features.Auth.DTOs
{
    public sealed class RecruiterRegisterRequestDTO
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;

        public string? Address { get; set; }

        [Required, MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        public string? CompanyDescription { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? CompanyIndustry { get; set; }
        public string? CompanySize { get; set; }
        public string? CompanyLocation { get; set; }
        public string? CompanyType { get; set; }
    }
}
