using Api.Services.Localizations;
using Application.Common.Interfaces.Services.Cache;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Api.common.Localizations.Json;

public class JsonStringLocalizerFactory(
    IMemoryCacheService cache,
    IOptions<LocalizationSettings> localizationSettings,
    IOptions<LocalizationOptions> localizationOptions
) : IStringLocalizerFactory
{
    private readonly string basePath = Path.Combine(
        Directory.GetCurrentDirectory(),
        localizationOptions.Value.ResourcesPath ?? "Resources"
    );
    private readonly IMemoryCacheService cache = cache;

    public IStringLocalizer Create(Type resourceSource)
    {
        return CreateLocalizer(resourceSource.Name);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        return CreateLocalizer();
    }

    private JsonStringLocalizer CreateLocalizer(string? subPath = null)
    {
        JsonLocalizationLoader loader = new(cache, localizationSettings, basePath, subPath!);
        return new JsonStringLocalizer(loader, localizationSettings);
    }
}
