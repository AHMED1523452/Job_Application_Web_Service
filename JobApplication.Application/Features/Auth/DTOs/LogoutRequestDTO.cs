namespace JobApplication.Application.Features.Auth.DTOs
{
    public class LogoutRequestDTO
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
