namespace Application.Common.Interfaces.Services.Localization;

public interface ITranslatorService
{
    public string Translate(string key, string? prefix = null);
}
