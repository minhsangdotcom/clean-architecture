using System.Linq.Expressions;
using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Repositories.EfCore;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.Create;
using Bogus;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;

namespace Application.UnitTest.Users;

public partial class CreateUserCommandValidatorTest
{
    private readonly CreateUserCommand command = null!;
    private readonly InlineValidator<CreateUserCommand> inlineValidator;
    private readonly CreateUserCommandValidator validator;

    private readonly Mock<IMessageTranslatorService> translator = new();
    private readonly Mock<IEfUnitOfWork> unitOfWork = new();

    private readonly Mock<IEfAsyncRepository<Role>> roleRepo = new();
    private readonly Mock<IEfAsyncRepository<User>> userRepo = new();
    private readonly Mock<IEfAsyncRepository<Permission>> permissionRepo = new();

    public CreateUserCommandValidatorTest()
    {
        unitOfWork.Setup(x => x.Repository<Role>()).Returns(roleRepo.Object);
        unitOfWork.Setup(x => x.Repository<User>()).Returns(userRepo.Object);
        unitOfWork.Setup(x => x.Repository<Permission>()).Returns(permissionRepo.Object);

        Mock<IRequestContextProvider> contextProvider = new();
        validator = new(unitOfWork.Object, translator.Object, contextProvider.Object);
        inlineValidator = [];

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
        FakeRoleAndPermissionFound();

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
        command.LastName = new string('X', 300);
        translator.SetupTranslate(
            UserErrorMessages.UserLastNameTooLong,
            SharedResource.TranslateText
        );
        FakeRoleAndPermissionFound();

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
        //Arrange
        command.LastName = "Nguyen";

        //Act
        var result = await validator.TestValidateAsync(command);
        //Assert
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
        FakeRoleAndPermissionFound();

        //Act
        var result = await validator.TestValidateAsync(command);

        //Assert
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
        //Arrange
        command.FirstName = new string('X', 300);
        translator.SetupTranslate(
            UserErrorMessages.UserFirstNameTooLong,
            SharedResource.TranslateText
        );
        FakeRoleAndPermissionFound();

        //Act
        var result = await validator.TestValidateAsync(command);

        //Assert
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
        //Arrange
        command.FirstName = "Minh";

        //Act
        var result = await validator.TestValidateAsync(command);

        //Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }
    #endregion

    #region PHONE NUMBER RULES
    [Fact]
    public async Task Validate_When_PhoneNumberIsNullOrEmpty_Should_HaveError()
    {
        //Arrange
        command.PhoneNumber = null;

        //Act
        var result = await validator.TestValidateAsync(command);

        //Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("123456")]
    [InlineData("1234567890123456")]
    [InlineData("123 4567")]
    [InlineData("123-4567")]
    [InlineData("(123)4567")]
    [InlineData("abc12345")]
    [InlineData("+12345abcd")]
    [InlineData("++1234567")]
    [InlineData("123+4567")]
    public async Task Validate_When_PhoneNumberInvalid_Should_HaveError(string? phoneNumber)
    {
        //Arrange
        var expected = new ErrorReason(
            UserErrorMessages.UserPhoneNumberInvalid,
            SharedResource.TranslateText
        );
        translator.SetupTranslate(
            UserErrorMessages.UserPhoneNumberInvalid,
            SharedResource.TranslateText
        );

        inlineValidator
            .RuleFor(x => x.PhoneNumber)
            .Must(x => false)
            .When(x => true)
            .WithState(_ => expected);

        command.PhoneNumber = phoneNumber;

        //Act
        var result = await inlineValidator.TestValidateAsync(command);

        //Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_PhoneNumberIsValid_Should_Pass()
    {
        // Arrange
        inlineValidator
            .RuleFor(x => x.PhoneNumber)
            .Must(x => true)
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        command.PhoneNumber = "0968123456";

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

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
        FakeRoleAndPermissionFound();

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
    [InlineData("user@domain")]
    [InlineData("user@@mail.com")]
    public async Task Validate_When_EmailInvalid_Should_HaveError(string email)
    {
        // Arrange
        command.Email = email;
        translator.SetupTranslate(UserErrorMessages.UserEmailInvalid, SharedResource.TranslateText);
        FakeRoleAndPermissionFound();

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
        inlineValidator
            .RuleFor(x => x.Email)
            .MustAsync((email, _) => Task.FromResult(false))
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserEmailExistent,
                SharedResource.TranslateText
            ));

        command.Email = "test@example.com";

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
    public async Task Validate_When_EmailUnique_Should_Pass()
    {
        // Arrange
        inlineValidator
            .RuleFor(x => x.Email)
            .MustAsync((email, _) => Task.FromResult(true))
            .When(_ => true);

        command.Email = "unique@example.com";

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_When_EmailFormatValid_Should_Pass()
    {
        // Arrange
        command.Email = "valid.user@example.com";

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
    #endregion

    #region STATUS RULES
    [Theory]
    [InlineData(4)]
    [InlineData(3)]
    public async Task Validate_When_StatusNotInEnum_Should_HaveError(int status)
    {
        // Arrange
        command.Status = (UserStatus)status;
        translator.SetupTranslate(
            UserErrorMessages.UserStatusNotInEnum,
            SharedResource.TranslateText
        );
        FakeRoleAndPermissionFound();

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        var expected = new ErrorReason(
            UserErrorMessages.UserStatusNotInEnum,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.Status)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_StatusIsValidEnum_Should_Pass()
    {
        // Arrange
        command.Status = UserStatus.Active;

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }
    #endregion

    #region ROLES RULES
    [Theory]
    [InlineData(null)]
    [InlineData(default(List<Ulid>))]
    public async Task Validate_When_RolesNullOrEmpty_Should_HaveError(List<Ulid>? roles)
    {
        // Arrange
        command.Roles = roles;
        translator.SetupTranslate(
            UserErrorMessages.UserRolesRequired,
            SharedResource.TranslateText
        );
        permissionRepo
            .Setup(r =>
                r.CountAsync(
                    It.IsAny<Expression<Func<Permission, bool>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(command.Permissions!.Count);

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        var expected = new ErrorReason(
            UserErrorMessages.UserRolesRequired,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.Roles)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_RolesNotUnique_Should_HaveError()
    {
        // Arrange
        Ulid id = Ulid.NewUlid();
        command.Roles = [id, id];

        translator.SetupTranslate(
            UserErrorMessages.UserRolesNotUnique,
            SharedResource.TranslateText
        );
        permissionRepo
            .Setup(r =>
                r.CountAsync(
                    It.IsAny<Expression<Func<Permission, bool>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(command.Permissions!.Count);

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        var expected = new ErrorReason(
            UserErrorMessages.UserRolesNotUnique,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.Roles)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_RolesNotFoundInDb_Should_HaveError()
    {
        // Arrange
        var expected = new ErrorReason(
            UserErrorMessages.UserRolesNotFound,
            SharedResource.TranslateText
        );
        translator.SetupTranslate(
            UserErrorMessages.UserRolesNotFound,
            SharedResource.TranslateText
        );

        inlineValidator
            .RuleFor(x => x.Roles)
            .MustAsync((ids, _) => Task.FromResult(false))
            .When(_ => true)
            .WithState(_ => expected);

        command.Roles = [Ulid.NewUlid()];

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Roles)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_RolesValid_Should_Pass()
    {
        // Arrange
        inlineValidator
            .RuleFor(x => x.Roles)
            .MustAsync((ids, _) => Task.FromResult(true))
            .When(_ => true);

        command.Roles = [Ulid.NewUlid()];

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Roles);
    }
    #endregion

    #region PERMISSIONS RULES
    [Theory]
    [InlineData(null)]
    [InlineData(default(List<Ulid>))]
    public async Task Validate_When_PermissionsIsNullOrEmpty_Should_HaveError(
        List<Ulid>? permissionIds
    )
    {
        // Arrange
        command.Permissions = permissionIds;

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Permissions);
    }

    [Fact]
    public async Task Validate_When_PermissionsNotUnique_Should_HaveError()
    {
        // Arrange
        Ulid id = Ulid.NewUlid();
        command.Permissions = [id, id];

        translator.SetupTranslate(
            UserErrorMessages.UserPermissionsNotUnique,
            SharedResource.TranslateText
        );
        FakeRoleAndPermissionFound();

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        var expected = new ErrorReason(
            UserErrorMessages.UserPermissionsNotUnique,
            SharedResource.TranslateText
        );
        result
            .ShouldHaveValidationErrorFor(x => x.Permissions)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_PermissionsUnique_Should_Pass()
    {
        // Arrange
        Ulid id = Ulid.NewUlid();
        command.Permissions = [id];
        inlineValidator.RuleFor(x => x.Permissions).Must((x, ct) => true);

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Permissions);
    }

    [Fact]
    public async Task Validate_When_PermissionsNotFoundInDb_Should_HaveError()
    {
        // Arrange
        command.Permissions = [Ulid.NewUlid()];
        var expected = new ErrorReason(
            UserErrorMessages.UserPermissionsNotFound,
            SharedResource.TranslateText
        );
        translator.SetupTranslate(
            UserErrorMessages.UserPermissionsNotFound,
            SharedResource.TranslateText
        );

        inlineValidator
            .RuleFor(x => x.Permissions)
            .MustAsync((p, _) => Task.FromResult(false))
            .WithState(_ => expected);

        command.Permissions = [Ulid.NewUlid()];

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Permissions)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_PermissionsValid_Should_Pass()
    {
        // Arrange
        inlineValidator.RuleFor(x => x.Permissions).MustAsync((p, _) => Task.FromResult(true));

        command.Permissions = [Ulid.NewUlid()];

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Permissions);
    }
    #endregion

    private void FakeRoleAndPermissionFound()
    {
        roleRepo
            .Setup(r =>
                r.CountAsync(
                    It.IsAny<Expression<Func<Role, bool>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(command.Roles!.Count);
        permissionRepo
            .Setup(r =>
                r.CountAsync(
                    It.IsAny<Expression<Func<Permission, bool>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(command.Permissions!.Count);
    }

    public static CreateUserCommand Default =>
        new Faker<CreateUserCommand>()
            .RuleFor(x => x.FirstName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Name.LastName())
            .RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber("0#########"))
            .RuleFor(x => x.DateOfBirth, f => f.Date.Past(30))
            .RuleFor(x => x.Avatar, _ => null)
            .RuleFor(x => x.Status, f => f.PickRandom<UserStatus>())
            .RuleFor(x => x.Roles, f => [Ulid.NewUlid()])
            .RuleFor(x => x.Permissions, f => [Ulid.NewUlid()])
            .RuleFor(x => x.Username, f => f.Internet.UserName())
            .RuleFor(x => x.Email, f => f.Internet.Email())
            .RuleFor(x => x.Password, "Admin@123")
            .RuleFor(
                x => x.Gender,
                f => f.PickRandom(new Gender?[] { Gender.Male, Gender.Female, null })
            )
            .Generate();
}
