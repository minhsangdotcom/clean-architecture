using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Messages;
using Application.Features.Users.Commands.Update;
using AutoFixture;
using Domain.Aggregates.Users;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;

namespace Application.UnitTest.Users;

public class UpdateUserCommandValidatorTest
{
    private readonly UserUpdateRequest userUpdate;
    private readonly Fixture fixture = new();

    private readonly UpdateUserCommandValidator validator;
    private readonly InlineValidator<UserUpdateRequest> mockValidator = [];

    private readonly Mock<IMessageTranslatorService> translator = new();

    public UpdateUserCommandValidatorTest()
    {
        Mock<IEfUnitOfWork> unitOfWork = new();
        validator = new(unitOfWork.Object, translator.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenFirstNameNullOrEmpty_ShouldReturnNullFailure(string? firstName)
    {
        userUpdate!.FirstName = firstName;

        var result = await validator.TestValidateAsync(userUpdate);

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.FirstName)
            .Negative()
            .WithError(MessageErrorType.Required)
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
        userUpdate!.FirstName = new string([.. fixture.CreateMany<char>(257)]);

        var result = await validator.TestValidateAsync(userUpdate);

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
        userUpdate!.LastName = lastName;

        var result = await validator.TestValidateAsync(userUpdate);

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
        userUpdate!.LastName = new string([.. fixture.CreateMany<char>(257)]);

        var result = await validator.TestValidateAsync(userUpdate);

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
        userUpdate!.PhoneNumber = phoneNumber;

        var result = await validator.TestValidateAsync(userUpdate);

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
        userUpdate!.PhoneNumber = phoneNumber;

        var result = await validator.TestValidateAsync(userUpdate);

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.PhoneNumber)
            .Negative()
            .WithError(MessageErrorType.Valid)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Object.Translate(errorMessage));

        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_WhenRolesNull_ShouldReturnNullFailure()
    {
        userUpdate!.Roles = null;

        var result = await validator.TestValidateAsync(userUpdate);

        string errorMessage = Messenger
            .Create<UserUpdateRequest>(nameof(User))
            .Property(x => x.Roles!)
            .Negative()
            .WithError(MessageErrorType.Required)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Object.Translate(errorMessage));

        result
            .ShouldHaveValidationErrorFor(x => x.Roles)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }
}
