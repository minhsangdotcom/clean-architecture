using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Repositories.EfCore;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.Profiles;
using Bogus;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;

namespace Application.UnitTest.Users;

public class UpdateUserProfileCommandValidatorTest
{
    private readonly UpdateUserProfileCommand command = null!;

    private readonly UpdateUserProfileCommandValidator validator;
    private readonly InlineValidator<UpdateUserProfileCommand> inlineValidator = [];
    private readonly Mock<IMessageTranslatorService> translator = new();
    private readonly Mock<IEfRepository<User>> userRepo = new();

    public UpdateUserProfileCommandValidatorTest()
    {
        Mock<IEfUnitOfWork> unitOfWork = new();
        unitOfWork.Setup(x => x.Repository<User>()).Returns(userRepo.Object);

        Mock<IRequestContextProvider> contextProvider = new();
        Mock<ICurrentUser> currentUser = new();
        currentUser.Setup(x => x.Id).Returns(Ulid.NewUlid());
        validator = new(
            unitOfWork.Object,
            currentUser.Object,
            contextProvider.Object,
            translator.Object
        );
        command = Default;
    }

    #region LAST NAME RULES
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_When_LastNameIsNullOrEmpty_Should_HaveError(string? lastName)
    {
        // Arrange
        command.LastName = lastName;
        translator.SetupTranslate(
            UserErrorMessages.UserLastNameRequired,
            SharedResource.TranslateText
        );

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        var expected = new ErrorReason(
            UserErrorMessages.UserLastNameRequired,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_LastNameTooLong_Should_HaveError()
    {
        // Arrange
        command.LastName = new string('L', 300);
        translator.SetupTranslate(
            UserErrorMessages.UserLastNameTooLong,
            SharedResource.TranslateText
        );

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        var expected = new ErrorReason(
            UserErrorMessages.UserLastNameTooLong,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_LastNameIsValid_Should_Pass()
    {
        // Arrange
        command.LastName = "Valid";

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }
    #endregion

    #region FIRST NAME RULES
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_When_FirstNameIsNullOrEmpty_Should_HaveError(string? firstName)
    {
        // Arrange
        command.FirstName = firstName;
        translator.SetupTranslate(
            UserErrorMessages.UserFirstNameRequired,
            SharedResource.TranslateText
        );

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        var expected = new ErrorReason(
            UserErrorMessages.UserFirstNameRequired,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_FirstNameTooLong_Should_HaveError()
    {
        // Arrange
        command.FirstName = new string('F', 300);
        translator.SetupTranslate(
            UserErrorMessages.UserFirstNameTooLong,
            SharedResource.TranslateText
        );

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        var expected = new ErrorReason(
            UserErrorMessages.UserFirstNameTooLong,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_FirstNameIsValid_Should_Pass()
    {
        // Arrange
        command.FirstName = "Valid";

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }
    #endregion

    #region PHONE NUMBER RULES
    [Theory]
    [InlineData("123")]
    [InlineData("1234567890123456")]
    [InlineData("abc123456")]
    [InlineData("123-456789")]
    public async Task Validate_When_PhoneNumberInvalid_Should_HaveError(string phone)
    {
        // Arrange
        command.PhoneNumber = phone;

        translator.SetupTranslate(
            UserErrorMessages.UserPhoneNumberInvalid,
            SharedResource.TranslateText
        );

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        var expected = new ErrorReason(
            UserErrorMessages.UserPhoneNumberInvalid,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_PhoneNumberValid_Should_Pass()
    {
        // Arrange
        command.PhoneNumber = "0968123456";

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }
    #endregion

    #region EMAIL RULES
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
        var expected = new ErrorReason(
            UserErrorMessages.UserEmailRequired,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    [InlineData("user@@mail.com")]
    [InlineData("user@domain")]
    public async Task Validate_When_EmailInvalid_Should_HaveError(string email)
    {
        // Arrange
        command.Email = email;
        translator.SetupTranslate(UserErrorMessages.UserEmailInvalid, SharedResource.TranslateText);

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        var expected = new ErrorReason(
            UserErrorMessages.UserEmailInvalid,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_EmailAlreadyExists_Should_HaveError()
    {
        // Arrange
        translator.SetupTranslate(
            UserErrorMessages.UserEmailExistent,
            SharedResource.TranslateText
        );

        inlineValidator
            .RuleFor(x => x.Email)
            .MustAsync((email, _) => Task.FromResult(false))
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserEmailExistent,
                SharedResource.TranslateText
            ));

        command.Email = "duplicate@mail.com";

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert
        var expected = new ErrorReason(
            UserErrorMessages.UserEmailExistent,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_EmailIsValid_Should_Pass()
    {
        // Arrange
        command.Email = "valid@mail.com";

        inlineValidator.RuleFor(x => x.Email).MustAsync((email, _) => Task.FromResult(true));

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
    #endregion

    #region GENDER RULES
    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    public async Task Validate_When_GenderNotInEnum_Should_HaveError(int gender)
    {
        // Arrange
        command.Gender = (Gender)gender;

        translator.SetupTranslate(
            UserErrorMessages.UserGenderNotInEnum,
            SharedResource.TranslateText
        );

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        var expected = new ErrorReason(
            UserErrorMessages.UserGenderNotInEnum,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.Gender)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_GenderIsValid_Should_Pass()
    {
        // Arrange
        command.Gender = Gender.Male;

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Gender);
    }
    #endregion

    public static Faker<UpdateUserProfileCommand> Default =>
        new Faker<UpdateUserProfileCommand>()
            .RuleFor(x => x.FirstName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Name.LastName())
            .RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber("0#########"))
            .RuleFor(x => x.Email, f => f.Internet.Email())
            .RuleFor(x => x.DateOfBirth, f => f.Date.Past(25))
            .RuleFor(x => x.Avatar, _ => null)
            .RuleFor(x => x.Gender, Gender.Male);
}
