using JobApplication.Application.Auth.DTOs;
using JobApplication.Application.Patterns;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Interfaces
{
    public interface IAuthService
    {
        Task<GenericResult<RegisterResponseDTO>> RegisterAsync(RegisterRequestDTO request, CancellationToken cancellationToken);
        Task<GenericResult<LoginResponseDTO>> LoginAsync(LoginRequestDTO request, CancellationToken cancellationToken);
    }
}
