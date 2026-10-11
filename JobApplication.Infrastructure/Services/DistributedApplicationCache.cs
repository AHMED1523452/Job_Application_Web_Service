using System.Text.Json;
using JobApplication.Application.Interfaces;
using JobApplication.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace JobApplication.Infrastructure.Services;

public sealed class DistributedApplicationCache : IApplicationCache
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IConnectionMultiplexer _redis;
    private readonly DistributedCacheSettings _settings;
    private readonly ILogger<DistributedApplicationCache> _logger;

    public DistributedApplicationCache(
        IConnectionMultiplexer redis,
        IOptions<DistributedCacheSettings> settings,
        ILogger<DistributedApplicationCache> logger)
    {
        _redis = redis;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullKey = FullKey(key);
        try
        {
            var value = await _redis.GetDatabase().StringGetAsync(fullKey);
            if (value.IsNullOrEmpty) 
                return default;

            return JsonSerializer.Deserialize<T>(value.ToString(), SerializerOptions);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Redis cache read failed for key {CacheKey}.", fullKey);
            return default;
        }
    }

    public async Task CreateAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullKey = FullKey(key);
        try
        {

            var expiration = TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);
            var json = JsonSerializer.Serialize(value, SerializerOptions);
            await _redis.GetDatabase().StringSetAsync(fullKey, json, expiration);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Redis cache write failed for key {CacheKey}.", fullKey);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullKey = FullKey(key);
        try
        {
            await _redis.GetDatabase().KeyDeleteAsync(fullKey);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Redis cache removal failed for key {CacheKey}.", fullKey);
        }
    }

    public async Task  InvalidateAsync(string keyPrefix, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var pattern = $"{_settings.InstanceName}{keyPrefix}*";
        try
        {
            var database = _redis.GetDatabase();
            foreach (var endpoint in _redis.GetEndPoints()) //. redis endpoint is like  that (Ip/  Host + port) 
            {  
                //.stop the process if the user asked for that 
                cancellationToken.ThrowIfCancellationRequested();
                var server = _redis.GetServer(endpoint);

                //. redis server is like a redis instance that can be connected to the redis endpoint.
                //. skip the iteration if the server is not connected to the redis endpoint.
                if (!server.IsConnected) continue;

                //. if reached 500 factors refill from the start and clear the old factors 
                var keys = new List<RedisKey>(500); //. only in memory (don't mess this point)
                foreach (var redisKey in server.Keys(database.Database, pattern, pageSize: 500))
                {
                    keys.Add(redisKey);
                    if (keys.Count < 500) continue;
                    await database.KeyDeleteAsync(keys.ToArray());
                    keys.Clear(); 
                }

                if (keys.Count > 0)
                    await database.KeyDeleteAsync(keys.ToArray());
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Redis cache invalidation failed for key prefix {CachePrefix}.", pattern);
        }
    }

    //. HireSense
    private RedisKey FullKey(string key) => $"{_settings.InstanceName}:{key}";
}
