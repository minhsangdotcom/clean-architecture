using System.Globalization;
using Api.Services.Localizations;
using Microsoft.Extensions.Options;

namespace Api.Middlewares;

public class LocalizerMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IOptions<LocalizationSettings> options)
    {
        string? acceptLang = context.Request.Headers.AcceptLanguage.ToString();

        string? cultureKey = ExtractPrimaryLanguage(acceptLang);

        if (!string.IsNullOrWhiteSpace(cultureKey) && CultureExists(cultureKey))
        {
            var culture = new CultureInfo(cultureKey);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }
        else
        {
            var fallback = new CultureInfo(options.Value.DefaultCulture);
            CultureInfo.CurrentCulture = fallback;
            CultureInfo.CurrentUICulture = fallback;
        }

        await next(context);
    }

    private static string? ExtractPrimaryLanguage(string? header)
    {
        if (string.IsNullOrWhiteSpace(header))
            return null;

        // Example: "en-US,en;q=0.9"
        // Take before first comma → "en-US"
        string first = header.Split(',').First();

        try
        {
            // Use full language (e.g. "en-US")
            return new CultureInfo(first).Name;
        }
        catch
        {
            // Try two-letter code fallback
            if (first.Length >= 2)
                return first[..2];

            return null;
        }
    }

    private static bool CultureExists(string culture)
    {
        return CultureInfo
            .GetCultures(CultureTypes.AllCultures)
            .Any(c => c.Name.Equals(culture, StringComparison.OrdinalIgnoreCase));
    }
}
