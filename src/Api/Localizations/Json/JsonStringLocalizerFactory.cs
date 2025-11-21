using Microsoft.Extensions.Localization;

namespace Api.Localizations.Json;

public class JsonStringLocalizerFactory(IServiceProvider provider) : IStringLocalizerFactory
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
        JsonLocalizationLoader loader = ActivatorUtilities.CreateInstance<JsonLocalizationLoader>(
            provider,
            basePath
        );

        return new JsonStringLocalizer(loader);
    }
}
