namespace Application.Common.Interfaces.Services.Localization;

public interface ITranslator<TResource> : ITranslator;

public interface ITranslator
{
    public string Translate(string key);
    public string Translate(string key, params object[] args);
}
