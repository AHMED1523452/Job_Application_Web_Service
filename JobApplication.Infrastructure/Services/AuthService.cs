using Hangfire;
using JobApplication.Application.Features.Auth.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Patterns;
using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Identity;
using JobApplication.Infrastructure.Persistence;
using JobApplication.Infrastructure.Services.Email;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace JobApplication.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IGenerateJwtToken _jwtGenerator;
        private readonly IUnitOfWork unitOfWork;
        private readonly IPasswordHasher passwordHasher;
        private readonly IEmailNotificationQueue _emailNotificationQueue;

        public AuthService(UserManager<ApplicationUser> userManager,
                           RoleManager<IdentityRole<Guid>> roleManager,
                           IGenerateJwtToken jwtGenerator,
                           IUnitOfWork unitOfWork,
                           IPasswordHasher passwordHasher,
                           IEmailNotificationQueue emailNotificationQueue)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtGenerator = jwtGenerator;
            this.unitOfWork = unitOfWork;
            this.passwordHasher = passwordHasher;
            _emailNotificationQueue = emailNotificationQueue;
        }


        /// <summary>
        /// First,The Customer will register with his data 
        /// after that the Code will be created and will be sent to him and after that will upload the hashed code in db
        /// after that, we will make the user go login and get the access token with expire time 5 minutes 
        ///         and another token with expire date of 7 days and upload the hashed token in the db
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns> string as message </returns>
        public async Task<GenericResult<RegisterResponseDTO>> RegisterAsync(RegisterRequestDTO request, CancellationToken cancellationToken)
        {
            return await RegisterUserAsync(request, "User", null, cancellationToken);
        }

        public async Task<GenericResult<RegisterResponseDTO>> RegisterRecruiterAsync(
            RecruiterRegisterRequestDTO request,
            CancellationToken cancellationToken)
        {
            var account = new RegisterRequestDTO
            {
                FullName = request.FullName,
                Email = request.Email,
                Password = request.Password,
                Address = request.Address
            };
            var hrProfile = new HrProfile
            {
                Name = request.FullName,
                Address = request.Address,
                CompanyName = request.CompanyName,
                CompanyDescription = request.CompanyDescription,
                CompanyWebsite = request.CompanyWebsite,
                CompanyIndustry = request.CompanyIndustry,
                CompanySize = request.CompanySize,
                CompanyLocation = request.CompanyLocation,
                CompanyType = request.CompanyType
            };

            return await RegisterUserAsync(account, "Recruiter", hrProfile, cancellationToken);
        }

        public Task<GenericResult<RegisterResponseDTO>> RegisterAdminAsync(
            RegisterRequestDTO request,
            CancellationToken cancellationToken)
            => RegisterUserAsync(request, "Admin", null, cancellationToken);

        private async Task<GenericResult<RegisterResponseDTO>> RegisterUserAsync(
            RegisterRequestDTO request,
            string roleName,
            HrProfile? hrProfile,
            CancellationToken cancellationToken)
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
                //. returning the description if the process not completed
                result.IsSuccess = false;
                result.Message = string.Join(";", create.Errors.Select(e => e.Description));
                return result;
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _userManager.DeleteAsync(user);
                result.IsSuccess = false;
                result.Message = $"The {roleName} role is not configured.";
                return result;
            }

            var roleResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                result.IsSuccess = false;
                result.Message = string.Join(";", roleResult.Errors.Select(e => e.Description));
                return result;
            }

            if (hrProfile is not null)
            {
                hrProfile.UserId = user.Id;
                await unitOfWork.Repository<HrProfile>().AddAsync(hrProfile, cancellationToken);
            }

            // Create verification code and send email
            var rawCode = new Random().Next(100000, 999999).ToString();
            var hashed = "";
            try
            {
                // resolve via service provider would be better; keep fallback to BCrypt directly
                hashed = await passwordHasher.HashTokenAsync(rawCode, cancellationToken);
            }
            catch
            {
                hashed = rawCode; // fallback (not secure) if hasher missing
            }

            var codeRecord = new VerificationCode
            {
                Purpose = VerificationCodePurpose.EmailVerification,
                Email = request.Email,
                UserId = user.Id,
                CodeHash = hashed,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };

            await unitOfWork.Repository<VerificationCode>().AddAsync(codeRecord, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var html = OtpEmailHtmlBuilder.Build("Verify Your Email", "Use the verification code below to verify your email address.", rawCode);
            await _emailNotificationQueue.QueueAsync(
                user.Id, request.Email, "Verify your account.", html,
                cancellationToken: cancellationToken);

            // Return neutral message instructing user to check email
            result.IsSuccess = true;
            result.Message = "If the email you provided exists, a verification code has been sent. Please check your inbox.";
            result.Data = new RegisterResponseDTO(user.Id.ToString(), user.FullName, user.Email, string.Empty, string.Empty);
            return result;
        }


        //. Login Process
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

            if (user.IsRevoked || !user.IsActive)
            {
                result.IsSuccess = false;
                result.Message = "This account is not allowed to sign in.";
                return result;
            }

            // Require email confirmation
            if (!user.EmailConfirmed)
            {
                result.IsSuccess = false;
                result.Message = "Email not verified. Please verify your email before logging in.";
                return result;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            var access = await _jwtGenerator.GenerateAccessTokenAsync(user.Id.ToString(), user.Email, user.UserName, user.FullName, role, cancellationToken);

            // create refresh token (id.raw) and persist hashed raw
            var raw = await _jwtGenerator.GenerateRefreshTokenAsync(user.Id.ToString(), user.Email, user.UserName, user.FullName, role, cancellationToken);
            var hashed = await passwordHasher.HashTokenAsync(raw,cancellationToken);

            var refreshEntity = new RefreshToken
            {
                TokenHash = hashed,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                UserId = user.Id
            };

            await unitOfWork.Repository<RefreshToken>().AddAsync(refreshEntity, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            result.IsSuccess = true;
            result.Data = new LoginResponseDTO(access, refreshEntity.Id + "." + raw);
            return result;
        }

        public async Task<GenericResult<string>> VerifyEmailAsync(VerifyEmailRequestDTO request, CancellationToken cancellationToken)
        {
            // Placeholder implementation to satisfy IAuthService.
            // Actual verification is handled by MediatR handlers; callers should use the handler or API endpoint.
            return await Task.FromResult(new GenericResult<string>
            {
                IsSuccess = false,
                Message = "Not implemented via service. Use /api/auth/verify-email endpoint."
            });
        }

        public async Task<GenericResult<LoginResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO request, CancellationToken cancellationToken)
        {
            var result = new GenericResult<LoginResponseDTO>();
            var parts = request.RefreshToken?.Split('.', 2);

            if (parts is null || parts.Length != 2 || !Guid.TryParse(parts[0], out var tokenId) || string.IsNullOrWhiteSpace(parts[1]))
            {
                result.IsSuccess = false;
                result.Message = "Invalid refresh token.";
                return result;
            }

            var stored = await unitOfWork.Repository<RefreshToken>()
                .GetByIdAsyncWithTracking(t => t.Id == tokenId, cancellationToken);

            if (stored is null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
            {
                result.IsSuccess = false;
                result.Message = "Invalid or expired refresh token.";
                return result;
            }

            var isValid = await passwordHasher.VerifyAsync(stored.TokenHash, parts[1], cancellationToken);
            if (!isValid)
            {
                result.IsSuccess = false;
                result.Message = "Invalid refresh token.";
                return result;
            }

            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;

            var user = await _userManager.FindByIdAsync(stored.UserId.ToString());
            if (user is null || !user.IsActive || user.IsRevoked)
            {
                await unitOfWork.Repository<RefreshToken>().EditAsync(stored, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                result.IsSuccess = false;
                result.Message = "Invalid refresh token.";
                return result;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";
            var access = await _jwtGenerator.GenerateAccessTokenAsync(user.Id.ToString(), user.Email!, user.UserName, user.FullName, role, cancellationToken);
            var newRaw = await _jwtGenerator.GenerateRefreshTokenAsync(user.Id.ToString(), user.Email!, user.UserName, user.FullName, role, cancellationToken);
            var hashed = await passwordHasher.HashTokenAsync(newRaw, cancellationToken);

            var newRefresh = new RefreshToken
            {
                TokenHash = hashed,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                UserId = user.Id
            };

            await unitOfWork.Repository<RefreshToken>().EditAsync(stored, cancellationToken);
            await unitOfWork.Repository<RefreshToken>().AddAsync(newRefresh, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            result.IsSuccess = true;
            result.Data = new LoginResponseDTO(access, newRefresh.Id + "." + newRaw);
            return result;
        }

        public async Task<GenericResult<string>> ForgotPasswordAsync(ForgotPasswordRequestDTO request, CancellationToken cancellationToken)
        {
            var result = new GenericResult<string>
            {
                IsSuccess = true,
                Message = "If the email you provided exists, a reset code has been sent. Please check your inbox."
            };

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null || !user.IsActive || user.IsRevoked)
                return result;

            var invalidatedAt = DateTime.UtcNow;
            await unitOfWork.Repository<VerificationCode>().ExecuteUpdateAsync(
                code => code.Email == request.Email
                    && code.Purpose == VerificationCodePurpose.PasswordReset
                    && !code.IsUsed,
                setters => setters
                    .SetProperty(code => code.IsUsed, true)
                    .SetProperty(code => code.UsedAt, invalidatedAt),
                cancellationToken);

            var rawCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var hashed = await passwordHasher.HashTokenAsync(rawCode, cancellationToken);

            await unitOfWork.Repository<VerificationCode>().AddAsync(new VerificationCode
            {
                Purpose = VerificationCodePurpose.PasswordReset,
                Email = request.Email,
                UserId = user.Id,
                CodeHash = hashed,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false
            }, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var html = OtpEmailHtmlBuilder.Build(
                "Reset Your Password",
                "Use the code below to reset your password. If you did not request this, you can ignore this email.",
                rawCode);
            await _emailNotificationQueue.QueueAsync(
                user.Id, request.Email, "Reset your password.", html,
                cancellationToken: cancellationToken);

            return result;
        }

        public async Task<GenericResult<string>> ResetPasswordAsync(ResetPasswordRequestDTO request, CancellationToken cancellationToken)
        {
            var result = new GenericResult<string>();

            var codes = await unitOfWork.Repository<VerificationCode>().GetAllAsync<VerificationCode>(
                predicate: c => c.Email == request.Email
                    && c.Purpose == VerificationCodePurpose.PasswordReset
                    && !c.IsUsed,
                selector: c => c,
                cancellationToken: cancellationToken);

            var codeRecord = codes.OrderByDescending(c => c.CreatedAt).FirstOrDefault();
            if (codeRecord is null)
            {
                result.IsSuccess = false;
                result.Message = "Invalid or expired reset code.";
                return result;
            }

            if (codeRecord.ExpiresAt < DateTime.UtcNow)
            {
                result.IsSuccess = false;
                result.Message = "Reset code expired.";
                return result;
            }

            var isValid = await passwordHasher.VerifyAsync(codeRecord.CodeHash, request.Code, cancellationToken);
            if (!isValid)
            {
                result.IsSuccess = false;
                result.Message = "Invalid or expired reset code.";
                return result;
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null || user.Id != codeRecord.UserId)
            {
                result.IsSuccess = false;
                result.Message = "Invalid or expired reset code.";
                return result;
            }

            var identityToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await _userManager.ResetPasswordAsync(user, identityToken, request.NewPassword);
            if (!reset.Succeeded)
            {
                result.IsSuccess = false;
                result.Message = string.Join("; ", reset.Errors.Select(e => e.Description));
                return result;
            }

            codeRecord.IsUsed = true;
            codeRecord.UsedAt = DateTime.UtcNow;
            await unitOfWork.Repository<VerificationCode>().EditAsync(codeRecord, cancellationToken);
            
            //. editing the code record to mark it as used
            //. and setting the used at date to the current date and time
            //. revoking all the refresh tokens for the user after resetting the password
            //. to prevent the user from using the old tokens
            await RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var confirmationHtml = OtpEmailHtmlBuilder.Build(
                "Password Changed",
                "Your password was reset successfully. If you did not do this, contact support immediately.",
                null);
            await _emailNotificationQueue.QueueAsync(
                user.Id, request.Email, "Your password was changed.", confirmationHtml,
                cancellationToken: cancellationToken);

            result.IsSuccess = true;
            result.Message = "Password reset successfully. You can now login.";
            return result;
        }

        public async Task<GenericResult<string>> LogoutAsync(LogoutRequestDTO request, CancellationToken cancellationToken)
        {
            var result = new GenericResult<string>();
            var parts = request.RefreshToken?.Split('.', 2);

            if (parts is null || parts.Length != 2 || !Guid.TryParse(parts[0], out var tokenId) || string.IsNullOrWhiteSpace(parts[1]))
            {
                result.IsSuccess = false;
                result.Message = "Invalid refresh token.";
                return result;
            }

            var stored = await unitOfWork.Repository<RefreshToken>()
                .GetByIdAsyncWithTracking(t => t.Id == tokenId, cancellationToken);

            if (stored is null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
            {
                result.IsSuccess = false;
                result.Message = "Invalid or expired refresh token.";
                return result;
            }

            var isValid = await passwordHasher.VerifyAsync(stored.TokenHash, parts[1], cancellationToken);
            if (!isValid)
            {
                result.IsSuccess = false;
                result.Message = "Invalid refresh token.";
                return result;
            }

            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;
            await unitOfWork.Repository<RefreshToken>().EditAsync(stored, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            result.IsSuccess = true;
            result.Message = "Logged out successfully.";
            return result;
        }

        public async Task<GenericResult<string>> RevokeUserAsync(RevokeUserRequestDTO request, CancellationToken cancellationToken)
        {
            var result = new GenericResult<string>();
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
            {
                result.IsSuccess = false;
                result.Message = "User not found.";
                return result;
            }

            if (user.IsRevoked)
            {
                result.IsSuccess = true;
                result.Message = "User is already revoked.";
                return result;
            }

            user.IsRevoked = true;
            user.IsActive = false;
            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                result.IsSuccess = false;
                result.Message = string.Join("; ", update.Errors.Select(e => e.Description));
                return result;
            }

            await RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var email = user.Email;
                var html = OtpEmailHtmlBuilder.Build(
                    "Account Suspended",
                    "Your account has been revoked. You can no longer sign in. Contact support if you believe this is a mistake.",
                    null);
                await _emailNotificationQueue.QueueAsync(
                    user.Id, email, "Your account was revoked.", html,
                    cancellationToken: cancellationToken);
            }

            result.IsSuccess = true;
            result.Message = "User revoked successfully.";
            return result;
        }

        public async Task<GenericResult<string>> UnRevokeUserAsync(UnRevokeUserRequestDTO request, CancellationToken cancellationToken)
        {
            var result = new GenericResult<string>();
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
            {
                result.IsSuccess = false;
                result.Message = "User not found.";
                return result;
            }

            user.IsRevoked = false;
            user.IsActive = true;
            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                result.IsSuccess = false;
                result.Message = string.Join("; ", update.Errors.Select(e => e.Description));
                return result;
            }

            /// checking the existing email , if exist sending mail with restoring message
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var email = user.Email;
                var html = OtpEmailHtmlBuilder.Build(
                    "Account Restored",
                    "Your account has been restored. You can sign in again.",
                    null);
                await _emailNotificationQueue.QueueAsync(
                    user.Id, email, "Your account was restored.", html,
                    cancellationToken: cancellationToken);
            }

            result.IsSuccess = true;
            result.Message = "User unrevoked successfully.";
            return result;
        }

        private async Task RevokeAllRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
        {
            var revokedAt = DateTime.UtcNow;
            await unitOfWork.Repository<RefreshToken>().ExecuteUpdateAsync(
                token => token.UserId == userId && !token.IsRevoked,
                setters => setters
                    .SetProperty(token => token.IsRevoked, true)
                    .SetProperty(token => token.RevokedAt, revokedAt),
                cancellationToken);
        }
    }
}
