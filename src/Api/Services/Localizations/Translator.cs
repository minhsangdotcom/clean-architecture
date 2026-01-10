using Application.Common.Interfaces.Services.Localization;
using Microsoft.Extensions.Localization;

namespace Api.Services.Localizations;

/// <summary>
/// Json based translation
/// </summary>
/// <typeparam name="TResource"></typeparam>
public class Translator<TResource> : ITranslator<TResource>
{
    private readonly IStringLocalizer<TResource> localizer;

    public Translator(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<TResource>>();
    }

    public string Translate(string key)
    {
        return localizer[key];
    }

    public string Translate(string key, params object[] args)
    {
        return localizer[key, args];
    }
}
