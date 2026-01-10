using Application.Common.Interfaces.Services.Localization;
using Moq;

namespace Application.UnitTest;

public static class SharedResource
{
    public const string TranslateText = "Translated text";

    public static void SetupTranslate(
        this Mock<IMessageTranslator> translator,
        string code,
        string translated
    )
    {
        translator.Setup(x => x.Translate(code, It.IsAny<string?>())).Returns(translated);
    }
}
