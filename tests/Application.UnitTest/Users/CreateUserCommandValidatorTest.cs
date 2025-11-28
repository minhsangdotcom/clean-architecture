using System.Text.RegularExpressions;
using Application.Common.Interfaces.Services.Localization;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Messages;
using Application.Features.Users.Commands.Create;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;

namespace Application.UnitTest.Users;

public partial class CreateUserCommandValidatorTest
{
    private readonly InlineValidator<CreateUserCommand> mockValidator;

    private readonly CreateUserCommand? command = null;
    private readonly Ulid roleId;
    private readonly Mock<IMessageTranslatorService> translatorMock = new();
    private readonly IMessageTranslatorService translator;

    public CreateUserCommandValidatorTest()
    {
        mockValidator = [];
        roleId = Ulid.Parse("01JS72XZJ6NFKFVWA9QM03RY5G");
        translator = translatorMock.Object;
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenFirstNameNullOrEmpty_ShouldReturnNullFailure(string? firstName)
    {
        // arrange
        command!.FirstName = firstName;

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.FirstName)
            .Negative()
            .WithError(MessageErrorType.Required)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator.RuleFor(x => x.FirstName).NotEmpty().WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_WhenInvalidLengthOfFirstName_ShouldReturnMaximumLengthFailure()
    {
        // arrange

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.FirstName)
            .WithError(MessageErrorType.TooLong)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator.RuleFor(x => x.FirstName).MaximumLength(256).WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
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
        // arrange
        command!.LastName = lastName;

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.LastName)
            .Negative()
            .WithError(MessageErrorType.Required)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator.RuleFor(x => x.LastName).NotEmpty().WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_WhenInvalidLengthOfLastName_ShouldReturnMaximumLengthFailure()
    {
        // arrange

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.LastName)
            .WithError(MessageErrorType.TooLong)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator.RuleFor(x => x.LastName).MaximumLength(256).WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenEmailNullOrEmpty_ShouldReturnNullFailure(string? email)
    {
        // arrange
        command!.Email = email;

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.Email)
            .Negative()
            .WithError(MessageErrorType.Required)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator.RuleFor(x => x.Email).NotEmpty().WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData("admin@gmail")]
    [InlineData("admingmail.com")]
    [InlineData("@gmail.com")]
    public async Task CreateUser_WhenEmailInvalidFormat_ShouldReturnInvalidFailure(string email)
    {
        // arrange
        command!.Email = email;

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.Email)
            .Negative()
            .WithError(MessageErrorType.Valid)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator
            .RuleFor(x => x.Email)
            .Must(x =>
            {
                Regex regex = EmailValidationRegex();
                return regex.IsMatch(x!);
            })
            .WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_WhenEmailDuplicated_ShouldReturnExistFailure()
    {
        // arrange
        const string existedEmail = "admin@gmail.com";
        command!.Email = existedEmail;

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.Email)
            .WithError(MessageErrorType.Existent)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator
            .RuleFor(x => x.Email)
            .MustAsync(
                (email, cancellationToken) =>
                    IsEmailAvailableAsync(email!, existedEmail, cancellationToken)
            )
            .When(_ => true)
            .WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenPhoneNumberNullOrEmpty_ShouldReturNullFailure(
        string? phoneNumber
    )
    {
        // arrange
        command!.PhoneNumber = phoneNumber;

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.PhoneNumber)
            .Negative()
            .WithError(MessageErrorType.Required)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator.RuleFor(x => x.PhoneNumber).NotEmpty().WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData("1234567890123456")]
    [InlineData("+12345")]
    public async Task Validate_WhenPhoneNumberInvalidFormat_ShouldReturnInvalidFailure(
        string phoneNumber
    )
    {
        // arrange
        command!.PhoneNumber = phoneNumber;

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.PhoneNumber)
            .Negative()
            .WithError(MessageErrorType.Valid)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator
            .RuleFor(x => x.PhoneNumber)
            .Must(x =>
            {
                Regex regex = PhoneValidationRegex();
                return regex.IsMatch(x!);
            })
            .WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenUsernameNullOrEmpty_ShouldReturnNullFailure(string? username)
    {
        // arrange
        command!.Username = username;

        string errorMessage = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Username!)
            .Negative()
            .WithError(MessageErrorType.Required)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator.RuleFor(x => x.Username).NotEmpty().WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.Username)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData("admin-super")]
    [InlineData("admin@super")]
    [InlineData("admin123!")]
    public async Task CreateUser_WhenInvalidUsername_ShouldReturnInValidFailure(string username)
    {
        // arrange
        command!.Username = username;

        string errorMessage = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Username!)
            .Negative()
            .WithError(MessageErrorType.Valid)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator
            .RuleFor(x => x.Username)
            .Must(
                (_, x) =>
                {
                    Regex regex = UsernameValidationRegex();
                    return regex.IsMatch(x!);
                }
            )
            .WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.Username)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_WhenUsernameDuplicated_ShouldReturnNotExistenceFailure()
    {
        // arrange
        string existedUserName = "admin";
        command!.Username = existedUserName;

        string errorMessage = Messenger
            .Create<User>()
            .Property(x => x.Username)
            .WithError(MessageErrorType.Existent)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator
            .RuleFor(x => x.Username)
            .MustAsync(
                (username, cancellationToken) =>
                    IsUsernameAvailableAsync(
                        username!,
                        existedUserName,
                        cancellationToken: cancellationToken
                    )
            )
            .WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.Username)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenPasswordNullOrEmpty_ShouldReturnNullFailure(string? password)
    {
        // arrange
        command!.Password = password;

        string errorMessage = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Password!)
            .Negative()
            .WithError(MessageErrorType.Required)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator.RuleFor(x => x.Password).NotEmpty().WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData("admin@123")]
    [InlineData("adminadmin")]
    [InlineData("admin")]
    public async Task Validate_WhenPasswordInvalid_ShouldReturnInvalidFailure(string password)
    {
        // arrange
        command!.Password = password;

        string errorMessage = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Password!)
            .Negative()
            .WithError(MessageErrorType.Strong)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator
            .RuleFor(x => x.Password)
            .Must(
                (_, x) =>
                {
                    Regex regex = PassowordValidationRegex();
                    return regex.IsMatch(x!);
                }
            )
            .WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task Validate_WhenGenderInvalid_ShouldReturnNotAmongTheAllowedOptionsFailure(
        int gender
    )
    {
        // arrange
        command!.Gender = (Gender)gender;

        string errorMessage = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Gender!)
            .Negative()
            .WithError(MessageErrorType.AmongTheAllowedOptions)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator.RuleFor(x => x.Gender).IsInEnum().WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.Gender)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task CreateUser_WhenStatusNull_ShouldReturnNullFailure()
    {
        // arrange
        command!.Status = 0;

        string errorMessage = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Status!)
            .Negative()
            .WithError(MessageErrorType.Required)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator.RuleFor(x => x.Status).NotEmpty().WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.Status)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public async Task Validate_WhenInvalidStatus_ShouldReturnNotAmongTheAllowedOptionsFailure(
        int status
    )
    {
        // arrange
        command!.Status = (UserStatus)status;

        string errorMessage = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Status!)
            .Negative()
            .WithError(MessageErrorType.AmongTheAllowedOptions)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator.RuleFor(x => x.Status).IsInEnum().WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.Status)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_WhenRolesNull_ShouldReturnNullFailure()
    {
        // arrange
        command!.Roles = null;

        string errorMessage = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Roles!)
            .Negative()
            .WithError(MessageErrorType.Required)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator.RuleFor(x => x.Roles).NotEmpty().WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.Roles)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_WhenDuplicatedRole_ShouldReturnNotUniqueFailure()
    {
        // arrange
        command!.Roles!.Add(roleId);

        string errorMessage = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Roles!)
            .Negative()
            .WithError(MessageErrorType.Unique)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator
            .RuleFor(x => x.Roles)
            .Must(x => x!.Distinct().Count() == x!.Count)
            .WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.Roles)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_WhenNotFoundRole_ShouldReturnNotFoundFailure()
    {
        // arrange
        command!.Roles!.Add(Ulid.NewUlid());
        List<Ulid> existedroles = [roleId, Ulid.NewUlid()];

        string errorMessage = Messenger
            .Create<CreateUserCommand>(nameof(User))
            .Property(x => x.Roles!)
            .Negative()
            .WithError(MessageErrorType.Found)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Translate(errorMessage));

        mockValidator
            .RuleFor(x => x.Roles)
            .MustAsync(
                (roles, cancellationToken) =>
                    IsRolesAvailableAsync(roles!, existedroles, cancellationToken)
            )
            .WithState(_ => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);

        // assert
        result
            .ShouldHaveValidationErrorFor(x => x.Roles)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    private static async Task<bool> IsEmailAvailableAsync(
        string email,
        string existedEmail,
        CancellationToken cancellationToken = default
    )
    {
        return await Task.Run(() => email != existedEmail, cancellationToken);
    }

    private static async Task<bool> IsRolesAvailableAsync(
        List<Ulid> roles,
        List<Ulid> existedRoles,
        CancellationToken cancellationToken = default
    )
    {
        return await Task.Run(() => existedRoles.All(x => roles.Contains(x)), cancellationToken);
    }

    private static async Task<bool> IsUsernameAvailableAsync(
        string username,
        string existedUsername,
        CancellationToken cancellationToken
    )
    {
        return await Task.Run(() => username != existedUsername, cancellationToken);
    }

    [GeneratedRegex(@"^\+?\d{7,15}$")]
    private static partial Regex PhoneValidationRegex();

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex EmailValidationRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9_.]+$")]
    private static partial Regex UsernameValidationRegex();

    [GeneratedRegex(@"^((?=\S*?[A-Z])(?=\S*?[a-z])(?=\S*?[0-9]).{8,})\S$")]
    private static partial Regex PassowordValidationRegex();
}
