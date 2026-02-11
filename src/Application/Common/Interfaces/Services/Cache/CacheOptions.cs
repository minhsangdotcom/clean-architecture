using Microsoft.Extensions.Primitives;

namespace Application.Common.Interfaces.Services.Cache;

public enum CacheExpirationType
{
    None, // no expiration
    Absolute, // expires after a fixed time
    Sliding, // expiry resets on each access
}

public class CacheOptions
{
    public CacheExpirationType ExpirationType { get; set; } = CacheExpirationType.Absolute;
    public TimeSpan? Expiration { get; set; }
    public IChangeToken? ChangeToken { get; set; }
}
