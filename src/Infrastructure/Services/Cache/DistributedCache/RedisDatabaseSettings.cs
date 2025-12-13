using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services.Cache.DistributedCache;

public class RedisDatabaseSettings
{
    [Required]
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 6379;

    public string? Password { get; set; }

    public int DefaultCacheExpirationInMinute { get; set; }

    public bool IsEnabled { get; set; }
}
