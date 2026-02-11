namespace Infrastructure.Services.Cache.DistributedCache;

public class RedisSettings
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 6379;

    public string? Password { get; set; }

    public int DefaultExpirationInMinutes { get; set; }

    public bool IsEnabled { get; set; }
}
