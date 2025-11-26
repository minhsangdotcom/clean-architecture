using Api.Services.Localizations;
using Application.Common.Interfaces.Services.Cache;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Api.common.Localizations.Json;

public class JsonStringLocalizerFactory(
    IMemoryCacheService cacheService,
    IOptions<LocalizationSettings> options
) : IStringLocalizerFactory
{
    private readonly string basePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources");

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
        JsonLocalizationLoader loader = new(cacheService, basePath, subPath!);
        return new JsonStringLocalizer(loader, options);
    }
}
