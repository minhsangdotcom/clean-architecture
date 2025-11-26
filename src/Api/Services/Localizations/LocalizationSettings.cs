namespace Api.Services.Localizations;

public class LocalizationSettings
{
    public string DefaultCulture { get; set; } = "vi";
    public string[] SupportedCultures { get; set; } = [];
    public int TranslationCacheInMinutes { get; set; } = 30;
}
