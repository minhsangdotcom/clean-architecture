namespace Api.Services.Localizations;

public class LocalizationSettings
{
    public string DefaultCulture { get; set; } = "vi";

    private string[]? _supportedCultures;
    public string[] SupportedCultures
    {
        get { return _supportedCultures!; }
        set
        {
            if (value == null)
            {
                _supportedCultures = ["en", "vi"];
                return;
            }

            string[] cleaned = [.. value.Where(x => !string.IsNullOrWhiteSpace(x.Trim()))];

            if (cleaned.Length == 0)
            {
                _supportedCultures = ["en", "vi"];
                return;
            }

            _supportedCultures = cleaned;
        }
    }
    public int TranslationCacheDurationInMinutes { get; set; } = 30;
}
