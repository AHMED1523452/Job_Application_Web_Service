using JobApplication.Application.Features.Users.DTOs;
using JobApplication.Application.Patterns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Interfaces
{
    /// <summary>
    /// Abstraction for user management operations.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get all users.
        /// </summary>
        Task<GenericResult<List<UserResponseDTO>>> GetAllUsersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user by ID.
        /// </summary>
        Task<GenericResult<UserResponseDTO>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update user details.
        /// </summary>
        Task<GenericResult<UserResponseDTO>> UpdateUserAsync(Guid userId, string fullName, string? address, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete user by ID.
        /// </summary>
        Task<GenericResult<bool>> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
