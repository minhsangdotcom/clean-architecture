using Api.Services.Localizations;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Api.common.Localizations.Json;

public class JsonStringLocalizerFactory(
    IServiceProvider serviceProvider,
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
        JsonLocalizationLoader loader = ActivatorUtilities.CreateInstance<JsonLocalizationLoader>(
            serviceProvider,
            basePath,
            subPath!
        );
        return new JsonStringLocalizer(loader, options);
    }
}
