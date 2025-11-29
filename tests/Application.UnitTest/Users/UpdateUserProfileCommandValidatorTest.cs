using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Messages;
using Application.Features.Users.Commands.Profiles;
using Domain.Aggregates.Users;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;

namespace Application.UnitTest.Users;

public class UpdateUserProfileCommandValidatorTest
{
    private readonly UpdateUserProfileCommand command = null!;

    private readonly UpdateUserProfileCommandValidator validator;
    private readonly InlineValidator<UpdateUserProfileCommand> mockValidator = [];
    private readonly Mock<IMessageTranslatorService> translator = new();

    public UpdateUserProfileCommandValidatorTest()
    {
        Mock<IEfUnitOfWork> unitOfWork = new();
        Mock<IHttpContextAccessorService> httpContext = new();
        Mock<ICurrentUser> currentUserService = new();
        validator = new(
            unitOfWork.Object,
            currentUserService.Object,
            httpContext.Object,
            translator.Object
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenFirstNameNullOrEmpty_ShouldReturnNullFailure(string? firstName)
    {
        command!.FirstName = firstName;

        var result = await validator.TestValidateAsync(command);

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.FirstName)
            .Negative()
            .WithError(MessageErrorType.Required) // Null → Required
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Object.Translate(errorMessage));

        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_WhenInvalidLengthOfFirstName_ShouldReturnMaximumLengthFailure()
    {
        var result = await validator.TestValidateAsync(command);

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.FirstName)
            .WithError(MessageErrorType.TooLong)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Object.Translate(errorMessage));

        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenLastNameNullOrEmpty_ShouldReturnNullFailure(string? lastName)
    {
        command!.LastName = lastName;

        var result = await validator.TestValidateAsync(command);

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.LastName)
            .Negative()
            .WithError(MessageErrorType.Required)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Object.Translate(errorMessage));

        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_WhenInvalidLengthOfLastName_ShouldReturnMaximumLengthFailure()
    {
        var result = await validator.TestValidateAsync(command);

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.LastName)
            .WithError(MessageErrorType.TooLong)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Object.Translate(errorMessage));

        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenPhoneNumberNullOrEmpty_ShouldReturnNullFailure(
        string? phoneNumber
    )
    {
        command!.PhoneNumber = phoneNumber;

        var result = await validator.TestValidateAsync(command);

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.PhoneNumber!)
            .Negative()
            .WithError(MessageErrorType.Required)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Object.Translate(errorMessage));

        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("++12345678")]
    public async Task Validate_WhenPhoneNumberInvalidFormat_ShouldReturnInvalidFailure(
        string phoneNumber
    )
    {
        command!.PhoneNumber = phoneNumber;

        var result = await validator.TestValidateAsync(command);

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.PhoneNumber!)
            .Negative()
            .WithError(MessageErrorType.Valid)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Object.Translate(errorMessage));

        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }
}
