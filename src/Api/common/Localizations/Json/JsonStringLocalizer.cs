using System.Globalization;
using Api.Services.Localizations;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Api.common.Localizations.Json;

public class JsonStringLocalizer(
    JsonLocalizationLoader loader,
    IOptions<LocalizationSettings> options
) : IStringLocalizer
{
    private readonly LocalizationSettings localizationSettings = options.Value;
    public LocalizedString this[string name]
    {
        get
        {
            string value = GetValue(name);
            return new LocalizedString(name, value, value == name);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            string formatted = string.Format(GetValue(name), arguments);
            return new LocalizedString(name, formatted);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        throw new NotImplementedException();
    }

    private string GetValue(string key)
    {
        string culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        string? value = loader.GetValue(key, culture);
        if (!string.IsNullOrEmpty(value))
        {
            return value;
        }

        string fallback = localizationSettings.DefaultCulture;
        string? fallbackValue = loader.GetValue(key, fallback);
        if (!string.IsNullOrEmpty(fallbackValue))
        {
            return fallbackValue;
        }

        return key;
    }
}
