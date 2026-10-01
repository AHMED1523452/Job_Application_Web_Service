namespace JobApplication.Application.Features.Auth.DTOs
{
    public class VerifyEmailRequestDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
