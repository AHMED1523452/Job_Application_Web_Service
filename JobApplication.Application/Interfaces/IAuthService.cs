using JobApplication.Application.Features.Auth.DTOs;
using JobApplication.Application.Patterns;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Interfaces
{
    public interface IAuthService
    {
        Task<GenericResult<RegisterResponseDTO>> RegisterAsync(RegisterRequestDTO request, CancellationToken cancellationToken);
        Task<GenericResult<RegisterResponseDTO>> RegisterRecruiterAsync(RecruiterRegisterRequestDTO request, CancellationToken cancellationToken);
        Task<GenericResult<RegisterResponseDTO>> RegisterAdminAsync(RegisterRequestDTO request, CancellationToken cancellationToken);
        Task<GenericResult<LoginResponseDTO>> LoginAsync(LoginRequestDTO request, CancellationToken cancellationToken);
        Task<GenericResult<string>> VerifyEmailAsync(VerifyEmailRequestDTO request, CancellationToken cancellationToken);
        Task<GenericResult<LoginResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO request, CancellationToken cancellationToken);
        Task<GenericResult<string>> ForgotPasswordAsync(ForgotPasswordRequestDTO request, CancellationToken cancellationToken);
        Task<GenericResult<string>> ResetPasswordAsync(ResetPasswordRequestDTO request, CancellationToken cancellationToken);
        Task<GenericResult<string>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDTO request, CancellationToken cancellationToken);
        Task<GenericResult<string>> LogoutAsync(LogoutRequestDTO request, CancellationToken cancellationToken);
        Task<GenericResult<string>> RevokeUserAsync(RevokeUserRequestDTO request, CancellationToken cancellationToken);
        Task<GenericResult<string>> UnRevokeUserAsync(UnRevokeUserRequestDTO request, CancellationToken cancellationToken);
    }
}
