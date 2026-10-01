using JobApplication.Application.Interfaces;
using BCrypt.Net;

namespace JobApplication.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    public async Task<string> HashTokenAsync(string password, CancellationToken cancellationToken)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public async Task<bool> VerifyAsync(string hashed, string value, CancellationToken cancellationToken)
    {
        return BCrypt.Net.BCrypt.Verify(value, hashed);
    }
}
