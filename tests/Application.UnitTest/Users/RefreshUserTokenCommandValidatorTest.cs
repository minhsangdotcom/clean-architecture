using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.Token;
using FluentValidation.TestHelper;
using Moq;

namespace Application.UnitTest.Users;

public class RefreshUserTokenCommandValidatorTest
{
    private readonly Mock<IMessageTranslatorService> translator = new();
    private readonly RefreshUserTokenCommandValidator validator;

    private readonly RefreshUserTokenCommand command = new() { RefreshToken = "valid_refresh_token_123" };

    public RefreshUserTokenCommandValidatorTest()
    {
        Mock<IRequestContextProvider> contextProvider = new();
        validator = new RefreshUserTokenCommandValidator(contextProvider.Object, translator.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_When_RefreshTokenIsNullOrEmpty_Should_HaveError(string? refreshToken)
    {
        // Arrange
        command.RefreshToken = refreshToken;

        translator.SetupTranslate(
            UserErrorMessages.UserRefreshTokenTokenRequired,
            SharedResource.TranslateText
        );

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        ErrorReason expectedState =
            new(UserErrorMessages.UserRefreshTokenTokenRequired, SharedResource.TranslateText);

        result
            .ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }
}
