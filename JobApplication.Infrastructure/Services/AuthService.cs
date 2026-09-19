using JobApplication.Application.Auth.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace JobApplication.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IGenerateJwtToken _jwtGenerator;
        private readonly IUnitOfWork unitOfWork;

        public AuthService(UserManager<ApplicationUser> userManager,
                           RoleManager<IdentityRole<Guid>> roleManager,
                           IGenerateJwtToken jwtGenerator, 
                           IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtGenerator = jwtGenerator;
            this.unitOfWork = unitOfWork;
        }

        public async Task<GenericResult<RegisterResponseDTO>> RegisterAsync(RegisterRequestDTO request, CancellationToken cancellationToken)
        {
            var result = new GenericResult<RegisterResponseDTO>();

            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing is not null)
            {
                result.IsSuccess = false;
                result.Message = "Email already registered.";
                return result;
            }

            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                FullName = request.FullName,
                Address = request.Address,
                IsActive = true
            };

            var create = await _userManager.CreateAsync(user, request.Password);
            if (!create.Succeeded)
            {
                result.IsSuccess = false;
                result.Message = string.Join(";", create.Errors.Select(e => e.Description));
                return result;
            }

            // ensure role exists
            var defaultRole = "User";
            if (!await _roleManager.RoleExistsAsync(defaultRole))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(defaultRole));
            }

            await _userManager.AddToRoleAsync(user, defaultRole);

            var access = await _jwtGenerator.GenerateAccessTokenAsync(user.Id.ToString(), user.Email, user.UserName, user.FullName, defaultRole, cancellationToken);
            var refresh = await _jwtGenerator.GenerateRefreshTokenAsync(user.Id.ToString(), user.Email, user.UserName, user.FullName, defaultRole, cancellationToken);

            // Optionally persist refresh token here using unitOfWork if desired.
            result.Data = new RegisterResponseDTO(user.Id.ToString(), user.FullName, user.Email, access, refresh);
            return result;
        }

        public async Task<GenericResult<LoginResponseDTO>> LoginAsync(LoginRequestDTO request, CancellationToken cancellationToken)
        {
            var result = new GenericResult<LoginResponseDTO>();

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                result.IsSuccess = false;
                result.Message = "Invalid credentials.";
                return result;
            }

            var valid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!valid)
            {
                result.IsSuccess = false;
                result.Message = "Invalid credentials.";
                return result;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            var access = await _jwtGenerator.GenerateAccessTokenAsync(user.Id.ToString(), user.Email, user.UserName, user.FullName, role, cancellationToken);
            var refresh = await _jwtGenerator.GenerateRefreshTokenAsync(user.Id.ToString(), user.Email, user.UserName, user.FullName, role, cancellationToken);

            result.Data = new LoginResponseDTO(access, refresh);
            return result;
        }
    }
}
