using Api.Middlewares;
using Microsoft.Extensions.Localization;

namespace Api.Extensions;

public static class LocalizationExtension
{
    public static IServiceCollection AddLocalizationConfigs(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        services.AddSingleton<
            IStringLocalizerFactory,
            Localizations.Json.JsonStringLocalizerFactory
        >();
        services.AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
        services.AddSingleton<LocalizerMiddleware>();

        var supportedCultures = new[] { "en", "vi" };

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.SetDefaultCulture("en");

            options.AddSupportedCultures(supportedCultures);
            options.AddSupportedUICultures(supportedCultures);
        });
        return services;
    }
}
