namespace Application.Contracts.Localization;

public interface ITranslatorService
{
    public string Translate(string key, string? prefix = null);
}
