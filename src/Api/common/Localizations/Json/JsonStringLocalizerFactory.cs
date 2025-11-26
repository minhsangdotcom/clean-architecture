using Application.Common.Interfaces.Services.Cache;
using Microsoft.Extensions.Localization;

namespace Api.common.Localizations.Json;

public class JsonStringLocalizerFactory(IMemoryCacheService cacheService) : IStringLocalizerFactory
{
    private readonly string basePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources");

    public IStringLocalizer Create(Type resourceSource)
    {
        return CreateLocalizer();
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        return CreateLocalizer();
    }

    private JsonStringLocalizer CreateLocalizer()
    {
        JsonLocalizationLoader loader = new(cacheService, basePath);
        return new JsonStringLocalizer(loader);
    }
}
