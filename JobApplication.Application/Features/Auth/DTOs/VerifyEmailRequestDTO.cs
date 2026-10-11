using System.ComponentModel.DataAnnotations;

namespace JobApplication.Application.Features.Auth.DTOs
{
    public class VerifyEmailRequestDTO
    {
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
