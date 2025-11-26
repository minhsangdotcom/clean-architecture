using Application.Contracts.Localization;
using Microsoft.Extensions.Localization;

namespace Api.Services.Localizations;

public class PermissionTranslatorService(IStringLocalizer<Permissions> stringLocalizer)
    : IPermissionTranslatorService
{
    public string Translate(string key, string? prefix = null)
    {
        string result = string.Empty;
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            result += $"{prefix}:{key}";
        }
        return stringLocalizer[result];
    }
}
