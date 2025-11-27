using Api.Services.Localizations;
using Application.Common.Interfaces.Services.Cache;
using DotNetCoreExtension.Extensions;
using Microsoft.Extensions.Options;

namespace Api.common.Localizations.Json;

public class JsonLocalizationLoader(
    IMemoryCacheService cache,
    IOptions<LocalizationSettings> options,
    string basePath,
    string? type = null
)
{
    private readonly LocalizationSettings settings = options.Value;

    public string? GetValue(string key, string culture)
    {
        string cacheKey = $"locale_{culture}_{key}";
        return cache.GetOrSet(
            key,
            () =>
            {
                return LoadKeyFromDisk(key, culture);
            },
            new CacheOptions()
            {
                ExpirationType = CacheExpirationType.Sliding,
                Expiration = TimeSpan.FromMinutes(settings.TranslationCacheInMinutes),
            }
        );
    }

    private string? LoadKeyFromDisk(string key, string culture)
    {
        if (!Directory.Exists(basePath))
        {
            return null;
        }
        string[] jsonFiles = Directory.GetFiles(
            basePath,
            string.IsNullOrWhiteSpace(type) ? $"*.{culture}.json" : $"{type}.{culture}.json",
            SearchOption.AllDirectories
        );

        foreach (string file in jsonFiles)
        {
            string json = File.ReadAllText(file);

            Dictionary<string, string>? data = SerializerExtension
                .Deserialize<Dictionary<string, string>?>(json)
                .Object;

            if (data?.TryGetValue(key, out string? value) is true)
            {
                return value;
            }
        }

        return null;
    }
}
