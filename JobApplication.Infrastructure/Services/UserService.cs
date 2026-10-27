using JobApplication.Application.Features.Users.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Infrastructure.Services
{
    /// <summary>
    /// Implementation of <see cref="IUserService"/> using ASP.NET Core Identity UserManager.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<GenericResult<List<UserResponseDTO>>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userManager.Users
                .Select(u => new UserResponseDTO(
                    u.Id,
                    u.Email ?? string.Empty,
                    u.FullName,
                    u.Address,
                    u.IsActive,
                    u.IsRevoked))
                .ToListAsync(cancellationToken);

            return await Result.SuccessAsync(users, "Users retrieved successfully.");
        }

        public async Task<GenericResult<UserResponseDTO>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
                return await Result.FailureAsync<UserResponseDTO>("User not found.");

            return await Result.SuccessAsync(new UserResponseDTO(
                user.Id,
                user.Email ?? string.Empty,
                user.FullName,
                user.Address,
                user.IsActive,
                user.IsRevoked), "User retrieved successfully.");
        }

        public async Task<GenericResult<UserResponseDTO>> UpdateUserAsync(
            Guid userId,
            string fullName,
            string? address,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
                return await Result.FailureAsync<UserResponseDTO>("User not found.");

            user.FullName = fullName;
            user.Address = address;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
                return await Result.FailureAsync<UserResponseDTO>(
                    string.Join("; ", updateResult.Errors.Select(e => e.Description)));

            return await Result.SuccessAsync(new UserResponseDTO(
                user.Id,
                user.Email ?? string.Empty,
                user.FullName,
                user.Address,
                user.IsActive,
                user.IsRevoked), "User updated successfully.");
        }

        public async Task<GenericResult<bool>> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
                return await Result.FailureAsync<bool>("User not found.");

            var deleteResult = await _userManager.DeleteAsync(user);

            if (!deleteResult.Succeeded)
                return await Result.FailureAsync<bool>(
                    string.Join("; ", deleteResult.Errors.Select(e => e.Description)));

            return await Result.SuccessAsync(true, "User deleted successfully.");
        }
    }
}
