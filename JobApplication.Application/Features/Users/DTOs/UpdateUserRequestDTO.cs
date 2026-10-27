namespace JobApplication.Application.Features.Users.DTOs
{
    public record UpdateUserRequestDTO(
        string FullName,
        string? Address
    );
}
