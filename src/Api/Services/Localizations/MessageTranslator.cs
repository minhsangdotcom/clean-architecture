using Application.Common.Interfaces.Services.Localization;
using Microsoft.Extensions.Localization;

namespace Api.Services.Localizations;

public class MessageTranslator : IMessageTranslator
{
    private readonly IStringLocalizer<Messages> stringLocalizer;

    public MessageTranslator(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        stringLocalizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<Messages>>();
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
