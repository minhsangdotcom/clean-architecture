namespace Application.Common.Interfaces.Services.Localization;

public interface ITranslator
{
    public string Translate(string key, string? prefix = null);
}
