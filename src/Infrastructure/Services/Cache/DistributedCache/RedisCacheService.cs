using Application.Common.Interfaces.Services.Cache;
using DotNetCoreExtension.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Services.Cache.DistributedCache;

public class RedisCacheService(
    IDatabase redis,
    IOptions<RedisDatabaseSettings> options,
    ILogger<RedisCacheService> logger
) : IDistributedCacheService
{
    private readonly RedisDatabaseSettings redisDatabaseSettings = options.Value;

    public T? GetOrSet<T>(string key, Func<T> func, CacheOptions? options = null)
    {
        return GetOrSetDefault(
            key,
            func,
            options
                ?? new CacheOptions()
                {
                    ExpirationType = CacheExpirationType.Absolute,
                    Expiration = TimeSpan.FromMinutes(
                        redisDatabaseSettings.DefaultCachingTimeInMinute
                    ),
                }
        );
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> task,
        CacheOptions? options = null
    )
    {
        return await GetOrSetDefaultAsync(
            key,
            task,
            options
                ?? new CacheOptions()
                {
                    ExpirationType = CacheExpirationType.Absolute,
                    Expiration = TimeSpan.FromMinutes(
                        redisDatabaseSettings.DefaultCachingTimeInMinute
                    ),
                }
        );
    }

    public void Remove(string key)
    {
        bool isSuccess = redis.KeyDelete(key);
        if (isSuccess)
        {
            logger.LogDebug("Redis KeyDelete {Key}", key);
        }
        else
        {
            logger.LogDebug("Redis KeyDelete {Key} failed", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        bool isSuccess = await redis.KeyDeleteAsync(key);

        if (isSuccess)
        {
            logger.LogDebug("Redis KeyDelete {Key}", key);
        }
        else
        {
            logger.LogDebug("Redis KeyDelete {Key} failed", key);
        }
    }

    private async Task<T?> GetOrSetDefaultAsync<T>(
        string key,
        Func<Task<T>> factory,
        CacheOptions options
    )
    {
        RedisValue cached = await redis.StringGetAsync(key);
        if (cached.HasValue)
        {
            logger.LogInformation("Redis HIT for {Key}", key);

            if (
                options.ExpirationType == CacheExpirationType.Sliding
                && options.Expiration.HasValue
            )
            {
                await redis.KeyExpireAsync(key, options.Expiration);
            }

            return SerializerExtension.Deserialize<T>(cached!).Object;
        }

        logger.LogInformation("Redis MISS for {Key}, invoking factory", key);

        T result = await factory();
        if (result == null)
        {
            logger.LogWarning("Factory returned null for {Key}. Not caching.", key);
            return default;
        }

        string json = SerializerExtension.Serialize(result).StringJson;

        TimeSpan? expiry = null;
        if (
            options.ExpirationType == CacheExpirationType.Absolute
            || options.ExpirationType == CacheExpirationType.Sliding
        )
        {
            expiry =
                options.Expiration
                ?? TimeSpan.FromMinutes(redisDatabaseSettings.DefaultCachingTimeInMinute);
        }

        await redis.StringSetAsync(key, json, expiry);

        string expiryText = expiry == null ? "Unlimited" : $"{expiry?.TotalMinutes} minutes";
        logger.LogInformation(
            "Cached {Key} with {Type} expiration: {ExpiryText}",
            key,
            options.ExpirationType,
            expiryText
        );

        return result;
    }

    private T? GetOrSetDefault<T>(string key, Func<T> factory, CacheOptions options)
    {
        RedisValue cached = redis.StringGet(key);
        if (cached.HasValue)
        {
            logger.LogInformation("Redis HIT for {Key}", key);

            if (
                options.ExpirationType == CacheExpirationType.Sliding
                && options.Expiration.HasValue
            )
            {
                redis.KeyExpire(key, options.Expiration);
            }

            return SerializerExtension.Deserialize<T>(cached!).Object;
        }

        logger.LogInformation("Redis MISS for {Key}, invoking factory", key);

        T result = factory();
        if (result == null)
        {
            logger.LogWarning("Factory returned null for {Key}. Not caching.", key);
            return default;
        }

        string json = SerializerExtension.Serialize(result).StringJson;

        TimeSpan? expiry = null;
        if (
            options.ExpirationType == CacheExpirationType.Absolute
            || options.ExpirationType == CacheExpirationType.Sliding
        )
        {
            expiry =
                options.Expiration
                ?? TimeSpan.FromMinutes(redisDatabaseSettings.DefaultCachingTimeInMinute);
        }

        redis.StringSet(key, json, expiry);

        string expiryText = expiry == null ? "Unlimited" : $"{expiry?.TotalMinutes} minutes";
        logger.LogInformation(
            "Cached {Key} with {Type} expiration: {ExpiryText}",
            key,
            options.ExpirationType,
            expiryText
        );

        return result;
    }
}
