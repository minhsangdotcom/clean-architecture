using Application.Contracts.Localization;
using Microsoft.Extensions.Localization;

namespace Api.Services.Localizations;

public class MessageTranslatorService : IMessageTranslatorService
{
    private readonly IStringLocalizer<Messages> stringLocalizer;

    public MessageTranslatorService(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        stringLocalizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<Messages>>();
    }

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
