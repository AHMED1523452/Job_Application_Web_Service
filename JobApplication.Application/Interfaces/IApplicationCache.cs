namespace JobApplication.Application.Interfaces;

/// <summary>Simple distributed-cache operations called explicitly by application handlers.</summary>
public interface IApplicationCache
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task CreateAsync<T>(string key, T value, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task InvalidateAsync(string keyPrefix, CancellationToken cancellationToken = default);
}
