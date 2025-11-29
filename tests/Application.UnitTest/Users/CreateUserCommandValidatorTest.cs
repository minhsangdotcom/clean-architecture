using System.Linq.Expressions;
using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Repositories.EfCore;
using Application.Common.Interfaces.Services;
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
using Microsoft.AspNetCore.Http;
using Moq;

namespace Application.UnitTest.Users;

public partial class CreateUserCommandValidatorTest
{
    private readonly CreateUserCommand command = null!;
    private readonly InlineValidator<CreateUserCommand> inlineValidator;
    private readonly CreateUserCommandValidator validator;

    private readonly Mock<IMessageTranslatorService> translator = new();
    private readonly Mock<IEfUnitOfWork> unitOfWork = new();

    private readonly Mock<IAsyncRepository<Role>> roleRepo = new();
    private readonly Mock<IAsyncRepository<User>> userRepo = new();
    private readonly Mock<IAsyncRepository<Permission>> permissionRepo = new();

    public CreateUserCommandValidatorTest()
    {
        unitOfWork.Setup(x => x.Repository<Role>()).Returns(roleRepo.Object);
        unitOfWork.Setup(x => x.Repository<User>()).Returns(userRepo.Object);
        unitOfWork.Setup(x => x.Repository<Permission>()).Returns(permissionRepo.Object);

        Mock<IHttpContextAccessorService> httpContext = new();
        validator = new(unitOfWork.Object, translator.Object, httpContext.Object);
        inlineValidator = [];

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
    public async Task Should_HaveError_When_LastNameTooLong()
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
    public async Task Should_Pass_When_LastNameIsValid()
    {
        //Arrange
        command.LastName = "Nguyen";

        //Act
        var result = await validator.TestValidateAsync(command);
        //Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    // ----------------------------------------------------------------------
    // FIRST NAME RULES
    // ----------------------------------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Should_HaveError_When_FirstNameIsNullOrEmpty(string? firstName)
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
    public async Task Should_HaveError_When_FirstNameTooLong()
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
    public async Task Should_Pass_When_FirstNameIsValid()
    {
        //Arrange
        command.FirstName = "Minh";

        //Act
        var result = await validator.TestValidateAsync(command);

        //Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    // ----------------------------------------------------------------------
    // PHONE NUMBER RULES
    // ----------------------------------------------------------------------

    [Fact]
    public async Task Should_Pass_When_PhoneNumberIsNullOrEmpty()
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
    [InlineData("123")] // too short
    [InlineData("123456")] // still too short
    [InlineData("1234567890123456")] // too long
    [InlineData("123 4567")] // space
    [InlineData("123-4567")] // symbol
    [InlineData("(123)4567")] // parentheses
    [InlineData("abc12345")] // letters
    [InlineData("+12345abcd")] // letters after +
    [InlineData("++1234567")] // multiple +
    [InlineData("123+4567")] // + not at beginning
    public async Task Should_HaveError_When_PhoneNumberInvalid(string phoneNumber)
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
    public async Task Should_Pass_When_PhoneNumberValid()
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

    // ----------------------------------------------------------------------
    // STATUS RULES
    // ----------------------------------------------------------------------

    [Theory]
    [InlineData(4)]
    [InlineData(3)]
    public async Task Should_HaveError_When_StatusNotInEnum(int status)
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
    public async Task Should_Pass_When_StatusIsValidEnum()
    {
        // Arrange
        command.Status = UserStatus.Active;

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    // ----------------------------------------------------------------------
    // ROLES RULES (DB)  → inlineValidator
    // ----------------------------------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData(default(List<Ulid>))]
    public async Task Should_HaveError_When_RolesNullOrEmpty(List<Ulid>? roles)
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
    public async Task Should_HaveError_When_RolesNotUnique()
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
    public async Task Should_HaveError_When_RolesNotFoundInDb()
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
    public async Task Should_Pass_When_RolesValid()
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

    // ----------------------------------------------------------------------
    // PERMISSIONS RULES (optional, DB) → inlineValidator
    // ----------------------------------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData(default(List<Ulid>))]
    public async Task Should_HaveError_When_PermissionsIsNull(List<Ulid>? permissionIds)
    {
        // Arrange
        command.Permissions = permissionIds;

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Permissions);
    }

    [Fact]
    public async Task Should_HaveError_When_PermissionsNotUnique()
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
    public async Task Should_HaveError_When_PermissionsNotFoundInDb()
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
    public async Task Should_Pass_When_PermissionsValid()
    {
        // Arrange
        inlineValidator.RuleFor(x => x.Permissions).MustAsync((p, _) => Task.FromResult(true));

        command.Permissions = [Ulid.NewUlid()];

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Permissions);
    }

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
            .RuleFor(x => x.Roles, f => new List<Ulid>() { Ulid.NewUlid() })
            .RuleFor(x => x.Permissions, f => new List<Ulid>() { Ulid.NewUlid() })
            .RuleFor(x => x.Username, f => f.Internet.UserName())
            .RuleFor(x => x.Email, f => f.Internet.Email())
            .RuleFor(x => x.Password, f => "Admin@123")
            .RuleFor(
                x => x.Gender,
                f => f.PickRandom(new Gender?[] { Gender.Male, Gender.Female, null })
            )
            .Generate();
}
