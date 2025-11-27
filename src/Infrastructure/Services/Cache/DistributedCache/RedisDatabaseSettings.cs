namespace Infrastructure.Services.Cache.DistributedCache;

public class RedisDatabaseSettings
{
    public string? Host { get; set; }

    public int? Port { get; set; }

    public string? Password { get; set; }

    public int DefaultCacheExpirationInMinute { get; set; }

    public bool IsEnabled { get; set; }
}
