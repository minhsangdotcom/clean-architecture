using System.Linq.Expressions;
using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Repositories.EfCore;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.Update;
using Bogus;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;

namespace Application.UnitTest.Users;

public class UpdateUserCommandValidatorTest
{
    private readonly UpdateUserCommand command = null!;
    private readonly InlineValidator<UpdateUserCommand> inlineValidator;
    private readonly UpdateUserCommandValidator validator;

    private readonly Mock<IMessageTranslatorService> translator = new();
    private readonly Mock<IEfUnitOfWork> unitOfWork = new();

    private readonly Mock<IEfAsyncRepository<Role>> roleRepo = new();
    private readonly Mock<IEfAsyncRepository<User>> userRepo = new();
    private readonly Mock<IEfAsyncRepository<Permission>> permissionRepo = new();

    public UpdateUserCommandValidatorTest()
    {
        unitOfWork.Setup(x => x.Repository<Role>()).Returns(roleRepo.Object);
        unitOfWork.Setup(x => x.Repository<User>()).Returns(userRepo.Object);
        unitOfWork.Setup(x => x.Repository<Permission>()).Returns(permissionRepo.Object);

        Mock<IRequestContextProvider> contextProvider = new();
        validator = new(unitOfWork.Object, contextProvider.Object, translator.Object);
        inlineValidator = [];

        command = Default();
    }

    #region LAST NAME RULES
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_When_LastNameIsNullOrEmpty_Should_HaveError(string? lastName)
    {
        // Arrange
        command.UpdateData.LastName = lastName;
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
            .ShouldHaveValidationErrorFor(x => x.UpdateData.LastName)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_LastNameTooLong_Should_HaveError()
    {
        // Arrange
        command.UpdateData.LastName = new string('X', 300);
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
            .ShouldHaveValidationErrorFor(x => x.UpdateData.LastName)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_LastNameIsValid_Should_Pass()
    {
        //Arrange
        command.UpdateData.LastName = "Nguyen";

        //Act
        var result = await validator.TestValidateAsync(command);

        //Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateData.LastName);
    }
    #endregion

    #region FIRST NAME RULES
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_When_FirstNameIsNullOrEmpty_Should_HaveError(string? firstName)
    {
        // Arrange
        command.UpdateData.FirstName = firstName;
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
            .ShouldHaveValidationErrorFor(x => x.UpdateData.FirstName)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_FirstNameTooLong_Should_HaveError()
    {
        //Arrange
        command.UpdateData.FirstName = new string('X', 300);
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
            .ShouldHaveValidationErrorFor(x => x.UpdateData.FirstName)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_FirstNameIsValid_Should_Pass()
    {
        //Arrange
        command.UpdateData.FirstName = "Minh";

        //Act
        var result = await validator.TestValidateAsync(command);

        //Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateData.FirstName);
    }
    #endregion

    #region PHONE NUMBER RULES
    [Fact]
    public async Task Validate_When_PhoneNumberIsNullOrEmpty_Should_Pass()
    {
        //Arrange
        command.UpdateData.PhoneNumber = null;

        //Act
        var result = await validator.TestValidateAsync(command);

        //Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateData.PhoneNumber);
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
            .RuleFor(x => x.UpdateData.PhoneNumber)
            .Must(x => false)
            .When(x => true)
            .WithState(_ => expected);

        command.UpdateData.PhoneNumber = phoneNumber;

        //Act
        var result = await inlineValidator.TestValidateAsync(command);

        //Assert
        result
            .ShouldHaveValidationErrorFor(x => x.UpdateData.PhoneNumber)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_PhoneNumberIsValid_Should_Pass()
    {
        // Arrange
        inlineValidator
            .RuleFor(x => x.UpdateData.PhoneNumber)
            .Must(x => true)
            .When(x => !string.IsNullOrEmpty(x.UpdateData.PhoneNumber));

        command.UpdateData.PhoneNumber = "0968123456";

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateData.PhoneNumber);
    }
    #endregion

    #region Email rule
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_When_EmailIsNullOrEmpty_Should_HaveError(string? email)
    {
        // Arrange
        command.UpdateData.Email = email;
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
            .ShouldHaveValidationErrorFor(x => x.UpdateData.Email)
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
        command.UpdateData.Email = email;
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
            .ShouldHaveValidationErrorFor(x => x.UpdateData.Email)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_EmailAlreadyExists_Should_HaveError()
    {
        // Arrange
        inlineValidator
            .RuleFor(x => x.UpdateData.Email)
            .MustAsync((email, _) => Task.FromResult(false))
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserEmailExistent,
                SharedResource.TranslateText
            ));

        command.UpdateData.Email = "test@example.com";

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert
        var expected = new ErrorReason(
            UserErrorMessages.UserEmailExistent,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.UpdateData.Email)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_EmailUnique_Should_Pass()
    {
        // Arrange
        inlineValidator
            .RuleFor(x => x.UpdateData.Email)
            .MustAsync((email, _) => Task.FromResult(true))
            .When(_ => true);

        command.UpdateData.Email = "unique@example.com";

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateData.Email);
    }

    [Fact]
    public async Task Validate_When_EmailFormatValid_Should_Pass()
    {
        // Arrange
        command.UpdateData.Email = "valid.user@example.com";

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateData.Email);
    }
    #endregion

    #region STATUS RULES
    [Theory]
    [InlineData(4)]
    [InlineData(3)]
    public async Task Validate_When_StatusNotInEnum_Should_HaveError(int status)
    {
        // Arrange
        command.UpdateData.Status = (UserStatus)status;
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
            .ShouldHaveValidationErrorFor(x => x.UpdateData.Status)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_StatusIsValidEnum_Should_Pass()
    {
        // Arrange
        command.UpdateData.Status = UserStatus.Active;

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateData.Status);
    }
    #endregion

    #region ROLES RULES
    [Theory]
    [InlineData(null)]
    public async Task Validate_When_RolesNullOrEmpty_Should_HaveError(List<Ulid>? roles)
    {
        // Arrange
        command.UpdateData.Roles = roles;
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
            .ReturnsAsync(command.UpdateData.Permissions!.Count);

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        var expected = new ErrorReason(
            UserErrorMessages.UserRolesRequired,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.UpdateData.Roles)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_RolesNotUnique_Should_HaveError()
    {
        // Arrange
        Ulid id = Ulid.NewUlid();
        command.UpdateData.Roles = [id, id];

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
            .ReturnsAsync(command.UpdateData.Permissions!.Count);

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        var expected = new ErrorReason(
            UserErrorMessages.UserRolesNotUnique,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.UpdateData.Roles)
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
            .RuleFor(x => x.UpdateData.Roles)
            .MustAsync((ids, _) => Task.FromResult(false))
            .When(_ => true)
            .WithState(_ => expected);

        command.UpdateData.Roles = [Ulid.NewUlid()];

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.UpdateData.Roles)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_RolesValid_Should_Pass()
    {
        // Arrange
        inlineValidator
            .RuleFor(x => x.UpdateData.Roles)
            .MustAsync((ids, _) => Task.FromResult(true))
            .When(_ => true);

        command.UpdateData.Roles = [Ulid.NewUlid()];

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateData.Roles);
    }
    #endregion

    #region PERMISSIONS RULES
    [Theory]
    [InlineData(null)]
    public async Task Validate_When_PermissionsIsNullOrEmpty_Should_HaveError(
        List<Ulid>? permissionIds
    )
    {
        // Arrange
        command.UpdateData.Permissions = permissionIds;

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateData.Permissions);
    }

    [Fact]
    public async Task Validate_When_PermissionsNotUnique_Should_HaveError()
    {
        // Arrange
        Ulid id = Ulid.NewUlid();
        command.UpdateData.Permissions = [id, id];

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
            .ShouldHaveValidationErrorFor(x => x.UpdateData.Permissions)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_PermissionsUnique_Should_Pass()
    {
        // Arrange
        Ulid id = Ulid.NewUlid();
        command.UpdateData.Permissions = [id];
        inlineValidator.RuleFor(x => x.UpdateData.Permissions).Must((x, ct) => true);

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert

        result.ShouldNotHaveValidationErrorFor(x => x.UpdateData.Permissions);
    }

    [Fact]
    public async Task Validate_When_PermissionsNotFoundInDb_Should_HaveError()
    {
        // Arrange
        command.UpdateData.Permissions = [Ulid.NewUlid()];
        var expected = new ErrorReason(
            UserErrorMessages.UserPermissionsNotFound,
            SharedResource.TranslateText
        );
        translator.SetupTranslate(
            UserErrorMessages.UserPermissionsNotFound,
            SharedResource.TranslateText
        );

        inlineValidator
            .RuleFor(x => x.UpdateData.Permissions)
            .MustAsync((p, _) => Task.FromResult(false))
            .WithState(_ => expected);

        command.UpdateData.Permissions = [Ulid.NewUlid()];

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.UpdateData.Permissions)
            .WithCustomState(expected, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_When_PermissionsValid_Should_Pass()
    {
        // Arrange
        inlineValidator
            .RuleFor(x => x.UpdateData.Permissions)
            .MustAsync((p, _) => Task.FromResult(true));

        command.UpdateData.Permissions = [Ulid.NewUlid()];

        // Act
        var result = await inlineValidator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateData.Permissions);
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
            .ReturnsAsync(command.UpdateData.Roles!.Count);
        permissionRepo
            .Setup(r =>
                r.CountAsync(
                    It.IsAny<Expression<Func<Permission, bool>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(command.UpdateData.Permissions!.Count);
    }

    private static UpdateUserCommand Default()
    {
        Faker<UserUpdateData> updateDataFaker = new Faker<UserUpdateData>()
            .RuleFor(x => x.FirstName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Name.LastName())
            .RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber("0#########"))
            .RuleFor(x => x.Email, f => f.Internet.Email())
            .RuleFor(x => x.DateOfBirth, f => f.Date.Past(30))
            .RuleFor(x => x.Avatar, _ => null)
            .RuleFor(x => x.Status, f => f.PickRandom<UserStatus>())
            .RuleFor(x => x.Roles, f => [Ulid.NewUlid(), Ulid.NewUlid()])
            .RuleFor(x => x.Permissions, f => [Ulid.NewUlid(), Ulid.NewUlid()]);

        return new Faker<UpdateUserCommand>()
            .RuleFor(x => x.UserId, f => Ulid.NewUlid().ToString())
            .RuleFor(x => x.UpdateData, _ => updateDataFaker.Generate())
            .Generate();
    }
}
