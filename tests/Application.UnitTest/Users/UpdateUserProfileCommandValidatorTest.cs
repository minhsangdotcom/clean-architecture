using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Repositories.EfCore;
using Application.Common.Interfaces.Services;
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
    private readonly Mock<IAsyncRepository<User>> userRepo = new();

    public UpdateUserProfileCommandValidatorTest()
    {
        Mock<IEfUnitOfWork> unitOfWork = new();
        unitOfWork.Setup(x => x.Repository<User>()).Returns(userRepo.Object);

        Mock<IHttpContextAccessorService> httpContext = new();
        Mock<ICurrentUser> currentUserService = new();
        currentUserService.Setup(x => x.Id).Returns(Ulid.NewUlid());
        validator = new(
            unitOfWork.Object,
            currentUserService.Object,
            httpContext.Object,
            translator.Object
        );
        command = Default;
    }

    // ----------------------------------------------------------------------
    // LAST NAME RULES
    // ----------------------------------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Should_HaveError_When_LastNameIsNullOrEmpty(string? lastName)
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
    public async Task Should_HaveError_When_LastNameTooLong()
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
    public async Task Should_Pass_When_LastNameValid()
    {
        // Arrange
        command.LastName = "Valid";

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    // ----------------------------------------------------------------------
    // FIRST NAME RULES
    // ----------------------------------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Should_HaveError_When_FirstNameNullOrEmpty(string? firstName)
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
    public async Task Should_HaveError_When_FirstNameTooLong()
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
    public async Task Should_Pass_When_FirstNameValid()
    {
        // Arrange
        command.FirstName = "Valid";

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    // ----------------------------------------------------------------------
    // PHONE NUMBER RULES
    // ----------------------------------------------------------------------

    [Theory]
    [InlineData("123")]
    [InlineData("1234567890123456")]
    [InlineData("abc123456")]
    [InlineData("123-456789")]
    public async Task Should_HaveError_When_PhoneNumberInvalid(string phone)
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
    public async Task Should_Pass_When_PhoneNumberValid()
    {
        // Arrange
        command.PhoneNumber = "0968123456";

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    // ----------------------------------------------------------------------
    // EMAIL RULES
    // ----------------------------------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Should_HaveError_When_EmailNullOrEmpty(string? email)
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
    public async Task Should_HaveError_When_EmailInvalid(string email)
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
    public async Task Should_HaveError_When_EmailAlreadyExists()
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
    public async Task Should_Pass_When_EmailValid()
    {
        // Arrange
        command.Email = "valid@mail.com";

        inlineValidator.RuleFor(x => x.Email).MustAsync((email, _) => Task.FromResult(true));

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    // ----------------------------------------------------------------------
    // GENDER RULES
    // ----------------------------------------------------------------------

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    public async Task Should_HaveError_When_GenderNotInEnum(int gender)
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
    public async Task Should_Pass_When_GenderValid()
    {
        // Arrange
        command.Gender = Gender.Male;

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Gender);
    }

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
