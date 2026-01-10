using Application.Common.Interfaces.Services.Localization;
using Moq;

namespace Application.UnitTest;

public static class SharedResource
{
    public const string TranslateText = "Translated text";

    public static void SetupTranslate(
        this Mock<ITranslator<Messages>> translator,
        string code,
        string translated
    )
    {
        translator.Setup(x => x.Translate(code)).Returns(translated);
    }
}
