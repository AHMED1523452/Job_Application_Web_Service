namespace JobApplication.Application.Interfaces;

public interface IPasswordHasher
{
    Task<string> HashTokenAsync(string password, CancellationToken cancellationToken);
    Task<bool> VerifyAsync(string hashed, string value, CancellationToken cancellationToken);
}
