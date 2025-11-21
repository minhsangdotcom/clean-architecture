using System.Globalization;
using Microsoft.Extensions.Localization;

namespace Api.Localizations.Json;

public class JsonStringLocalizer(JsonLocalizationLoader loader) : IStringLocalizer
{
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

        string? enValue = loader.GetValue(key, "en");
        return enValue ?? key;
    }
}
