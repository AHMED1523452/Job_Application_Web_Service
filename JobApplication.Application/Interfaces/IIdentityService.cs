
namespace JobApplication.Application.Interfaces
{
    public interface IIdentityService
    {
        Task<bool> ConfirmEmailAsync(string Email, CancellationToken cancellationToken);
        Task<UserContactInfo?> GetUserContactAsync(Guid userId, CancellationToken cancellationToken);
    }

    public sealed record UserContactInfo(Guid UserId, string Email, string FullName);
}
