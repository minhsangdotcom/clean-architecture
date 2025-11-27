using Application.Common.Interfaces.Services.Localization;
using Microsoft.Extensions.Localization;

namespace Api.Services.Localizations;

public class PermissionTranslatorService : IPermissionTranslatorService
{
    private readonly IStringLocalizer<Permissions> stringLocalizer;

    public PermissionTranslatorService(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        stringLocalizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<Permissions>>();
    }

    public string Translate(string key, string? prefix = null)
    {
        string result = string.Empty;
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            result += $"{prefix}:";
        }
        result += key;
        return stringLocalizer[result];
    }
}
