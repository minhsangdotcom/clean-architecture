using Application.Contracts.ApiWrapper;
using Application.Contracts.Localization;
using Application.Contracts.Messages;
using Application.Features.Users.Commands.ChangePassword;
using Domain.Aggregates.Users;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
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
        validator = new ChangeUserPasswordCommandValidator(translator.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_WhenOldPasswordIsNullOrEmpty_ShouldReturnNullFailure(
        string oldPassword
    )
    {
        //arrage
        command.OldPassword = oldPassword;

        //act
        var result = await validator.TestValidateAsync(command);

        //assert
        string errorMessage = Messenger
            .Create<ChangeUserPasswordCommand>(nameof(User))
            .Property(x => x.OldPassword!)
            .Negative()
            .WithError(MessageErrorType.Required)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Object.Translate(errorMessage));
        result
            .ShouldHaveValidationErrorFor(x => x.OldPassword)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_WhenNewPasswordNullOrEmpty_ShouldReturnNullFailure(string password)
    {
        //arrage
        command.NewPassword = password;

        //act
        var result = await validator.TestValidateAsync(command);

        //assert
        string errorMessage = Messenger
            .Create<ChangeUserPasswordCommand>(nameof(User))
            .Property(x => x.NewPassword!)
            .Negative()
            .WithError(MessageErrorType.Required)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Object.Translate(errorMessage));
        result
            .ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("admin@123")]
    [InlineData("admin0123")]
    public async Task Validate_WhenNewPasswordNotStrong_ShouldReturnNullFailure(string password)
    {
        //arrage
        command.NewPassword = password;

        //act
        var result = await validator.TestValidateAsync(command);

        //assert
        string errorMessage = Messenger
            .Create<ChangeUserPasswordCommand>(nameof(User))
            .Property(x => x.NewPassword!)
            .Negative()
            .WithError(MessageErrorType.Strong)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Object.Translate(errorMessage));

        result
            .ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }
}
