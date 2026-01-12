using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.RequestPasswordReset;
using FluentValidation.TestHelper;
using Moq;

namespace Application.UnitTest.Users;

public class RequestUserPasswordResetCommandValidatorTest
{
    private readonly Mock<ITranslator<Messages>> translator = new();
    private readonly RequestUserPasswordResetCommandValidator validator;

    private RequestUserPasswordResetCommand command = new("admin@gmail.com");

    public RequestUserPasswordResetCommandValidatorTest()
    {
        Mock<IRequestContextProvider> contextProvider = new();
        validator = new RequestUserPasswordResetCommandValidator(
            contextProvider.Object,
            translator.Object
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_When_EmailIsNullOrEmpty_Should_HaveError(string? email)
    {
        // Arrange
        command = command with
        {
            Email = email,
        };

        translator.SetupTranslate(
            UserErrorMessages.UserEmailRequired,
            SharedResource.TranslateText
        );

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        ErrorReason expectedState = new(
            UserErrorMessages.UserEmailRequired,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }
}
