using Microsoft.Extensions.Localization;
using Moq;

namespace Application.UnitTest.Extensions;

public class LocalizerMockHelper
{
    public static Mock<IStringLocalizer<T>> CreateStringLocalizerMock<T>()
    {
        var mock = new Mock<IStringLocalizer<T>>();

        mock.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        return mock;
    }
}
