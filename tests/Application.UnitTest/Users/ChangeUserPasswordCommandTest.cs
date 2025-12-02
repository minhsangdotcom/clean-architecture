using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.ChangePassword;
using FluentValidation.TestHelper;
using Moq;

namespace Application.UnitTest.Users;

public class ChangeUserPasswordCommandTest
{
    private readonly ChangeUserPasswordCommand command =
        new() { OldPassword = "Admin@123", NewPassword = "Admin@456" };

    private readonly Mock<IMessageTranslatorService> translator = new();
    private readonly ChangeUserPasswordCommandValidator validator;

    public ChangeUserPasswordCommandTest()
    {
        Mock<IRequestContextProvider> contextProvider = new();
        validator = new ChangeUserPasswordCommandValidator(
            contextProvider.Object,
            translator.Object
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_When_OldPasswordIsNullOrEmpty_Should_HaveError(string oldPassword)
    {
        //Arrange
        command.OldPassword = oldPassword;
        translator.SetupTranslate(
            UserErrorMessages.UserOldPasswordRequired,
            SharedResource.TranslateText
        );

        //act
        var result = await validator.TestValidateAsync(command);

        //assert
        ErrorReason expectedState =
            new(UserErrorMessages.UserOldPasswordRequired, SharedResource.TranslateText);
        result
            .ShouldHaveValidationErrorFor(x => x.OldPassword)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_When_NewPasswordIsNullOrEmpty_Should_HaveError(string password)
    {
        //Arrange
        command.NewPassword = password;
        translator.SetupTranslate(
            UserErrorMessages.UserNewPasswordRequired,
            SharedResource.TranslateText
        );

        //act
        var result = await validator.TestValidateAsync(command);

        //assert
        ErrorReason expectedState =
            new(UserErrorMessages.UserNewPasswordRequired, SharedResource.TranslateText);
        result
            .ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("admin@123")]
    [InlineData("admin0123")]
    public async Task Validate_When_NewPasswordIsNotStrong_Should_HaveError(string password)
    {
        //arrange
        command.NewPassword = password;
        translator.SetupTranslate(
            UserErrorMessages.UserNewPasswordNotStrong,
            SharedResource.TranslateText
        );

        //act
        var result = await validator.TestValidateAsync(command);

        //assert
        ErrorReason expectedState =
            new(UserErrorMessages.UserNewPasswordNotStrong, SharedResource.TranslateText);

        result
            .ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }
}
