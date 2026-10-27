using System;

namespace JobApplication.Application.Features.Users.DTOs
{
    public record UserResponseDTO(
        Guid Id,
        string Email,
        string FullName,
        string? Address,
        bool IsActive,
        bool IsRevoked
    );
}
