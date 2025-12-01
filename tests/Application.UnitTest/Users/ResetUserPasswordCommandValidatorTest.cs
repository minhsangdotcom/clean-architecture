using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.ResetPassword;
using FluentValidation.TestHelper;
using Moq;

namespace Application.UnitTest.Users;

public class ResetUserPasswordCommandValidatorTest
{
    private readonly Mock<IMessageTranslatorService> translator = new();
    private readonly Mock<IHttpContextAccessorService> httpContext = new();
    private readonly ResetUserPasswordCommand command =
        new()
        {
            Email = "admin@gmail.com",
            Token = "reset_token_123",
            Password = "Admin@123",
        };

    private readonly ResetUserPasswordCommandValidator validator;

    public ResetUserPasswordCommandValidatorTest()
    {
        validator = new ResetUserPasswordCommandValidator(httpContext.Object, translator.Object);
    }

    // ----------------------------
    // EMAIL: Required
    // ----------------------------
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_When_EmailIsNullOrEmpty_Should_HaveError(string? email)
    {
        // Arrange
        command.Email = email;

        translator.SetupTranslate(
            UserErrorMessages.UserEmailRequired,
            SharedResource.TranslateText
        );

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        ErrorReason expectedState =
            new(UserErrorMessages.UserEmailRequired, SharedResource.TranslateText);

        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    // ----------------------------
    // EMAIL: Invalid format
    // ----------------------------
    [Theory]
    [InlineData("invalid")]
    [InlineData("abc@")]
    [InlineData("123@com")]
    [InlineData("test@domain")]
    public async Task Validate_When_EmailIsInvalidFormat_Should_HaveError(string email)
    {
        // Arrange
        command.Email = email;

        translator.SetupTranslate(UserErrorMessages.UserEmailInvalid, SharedResource.TranslateText);

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        ErrorReason expectedState =
            new(UserErrorMessages.UserEmailInvalid, SharedResource.TranslateText);

        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    // ----------------------------
    // TOKEN: Required
    // ----------------------------
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_When_TokenIsNullOrEmpty_Should_HaveError(string? token)
    {
        // Arrange
        command.Token = token;

        translator.SetupTranslate(
            UserErrorMessages.UserPasswordResetTokenRequired,
            SharedResource.TranslateText
        );

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        ErrorReason expectedState =
            new(UserErrorMessages.UserPasswordResetTokenRequired, SharedResource.TranslateText);

        result
            .ShouldHaveValidationErrorFor(x => x.Token)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    // ----------------------------
    // PASSWORD: Required
    // ----------------------------
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_When_PasswordIsNullOrEmpty_Should_HaveError(string? password)
    {
        // Arrange
        command.Password = password;

        translator.SetupTranslate(
            UserErrorMessages.UserPasswordRequired,
            SharedResource.TranslateText
        );

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        ErrorReason expectedState =
            new(UserErrorMessages.UserPasswordRequired, SharedResource.TranslateText);

        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    // ----------------------------
    // PASSWORD: Weak format
    // ----------------------------
    [Theory]
    [InlineData("12345678")]
    [InlineData("password")]
    [InlineData("abcdefgh")]
    [InlineData("ABCDEF1234")]
    public async Task Validate_When_PasswordIsWeak_Should_HaveError(string password)
    {
        // Arrange
        command.Password = password;

        translator.SetupTranslate(
            UserErrorMessages.UserNewPasswordNotStrong,
            SharedResource.TranslateText
        );

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        ErrorReason expectedState =
            new(UserErrorMessages.UserNewPasswordNotStrong, SharedResource.TranslateText);

        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }
}
