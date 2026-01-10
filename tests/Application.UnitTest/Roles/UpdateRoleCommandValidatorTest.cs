using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Repositories.EfCore;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Features.Roles.Commands.Update;
using Bogus;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;

namespace Application.UnitTest.Roles;

public sealed class UpdateRoleCommandValidatorTest
{
    private readonly UpdateRoleCommandValidator validator;
    private readonly InlineValidator<UpdateRoleCommand> inlineValidator;
    private UpdateRoleCommand command = null!;

    private readonly Mock<IEfUnitOfWork> unitOfWork = new();

    private readonly Mock<IRequestContextProvider> contextProvider = new();
    private readonly Mock<IMessageTranslator> translator = new();

    public UpdateRoleCommandValidatorTest()
    {
        contextProvider.Setup(x => x.GetHttpMethod()).Returns(HttpMethod.Put.ToString());

        Mock<IEfRepository<Role>> roleRepo = new();
        Mock<IEfRepository<Permission>> permissionRepo = new();

        unitOfWork.Setup(x => x.Repository<Role>()).Returns(roleRepo.Object);
        unitOfWork.Setup(x => x.Repository<Permission>()).Returns(permissionRepo.Object);

        validator = new(unitOfWork.Object, contextProvider.Object, translator.Object);
        inlineValidator = [];
        ResetCommand();
    }

    private void ResetCommand()
    {
        var updateDataFaker = new Faker<RoleUpdateData>()
            .RuleFor(x => x.Name, f => f.Commerce.Department())
            .RuleFor(x => x.Description, f => f.Lorem.Sentence(8))
            .RuleFor(x => x.PermissionIds, f => [Ulid.NewUlid(), Ulid.NewUlid(), Ulid.NewUlid()]);
        var faker = new Faker<UpdateRoleCommand>().RuleFor(
            x => x.UpdateData,
            f => updateDataFaker.Generate()
        );

        command = faker.Generate();
    }

    #region Name Validation

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_When_NameIsNullOrEmpty_Should_HaveError(string? name)
    {
        //Arrange
        command.UpdateData.Name = name;
        translator.SetupTranslate(RoleErrorMessages.RoleNameRequired, SharedResource.TranslateText);

        //Act
        var result = await validator.TestValidateAsync(command.UpdateData);

        //Assert
        var expected = new ErrorReason(
            RoleErrorMessages.RoleNameRequired,
            SharedResource.TranslateText
        );
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithCustomState(expected, new ErrorReasonComparer());
    }

    [Fact]
    public async Task Validate_When_NameTooLong_Should_HaveError()
    {
        command.UpdateData.Name = new string('X', 300);
        translator.SetupTranslate(RoleErrorMessages.RoleNameTooLong, SharedResource.TranslateText);

        var result = await validator.TestValidateAsync(command.UpdateData);
        var expected = new ErrorReason(
            RoleErrorMessages.RoleNameTooLong,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithCustomState(expected, new ErrorReasonComparer());
    }

    [Fact]
    public async Task Validate_When_NameAlreadyExists_Should_HaveError()
    {
        //Arrange
        command.UpdateData.Name = "Admin";
        var expected = new ErrorReason(
            RoleErrorMessages.RoleNameExistent,
            SharedResource.TranslateText
        );

        inlineValidator
            .RuleFor(x => x.UpdateData.Name)
            .MustAsync((name, ct) => Task.FromResult(false))
            .When(_ => true, ApplyConditionTo.CurrentValidator)
            .WithState(_ => expected);

        //Act
        var result = await inlineValidator.TestValidateAsync(command);

        //Assert
        result
            .ShouldHaveValidationErrorFor(x => x.UpdateData.Name)
            .WithCustomState(expected, new ErrorReasonComparer());
    }

    [Fact]
    public async Task Validate_When_NameIsUnique_Should_Pass()
    {
        //Arrange
        command.UpdateData.Name = "Manager";

        inlineValidator
            .RuleFor(x => x.UpdateData.Name)
            .MustAsync((name, ct) => Task.FromResult(true))
            .When(_ => true, ApplyConditionTo.CurrentValidator);

        //Act
        var result = await inlineValidator.TestValidateAsync(command);

        //Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateData.Name);
    }

    #endregion


    #region Description Validation

    [Fact]
    public async Task Validate_When_DescriptionTooLong_Should_HaveError()
    {
        //Arrange
        command.UpdateData.Description = new string('D', 2000);
        translator.SetupTranslate(
            RoleErrorMessages.RoleDescriptionTooLong,
            SharedResource.TranslateText
        );

        //Act
        var result = await validator.TestValidateAsync(command.UpdateData);

        //Assert
        var expected = new ErrorReason(
            RoleErrorMessages.RoleDescriptionTooLong,
            SharedResource.TranslateText
        );
        result
            .ShouldHaveValidationErrorFor(x => x.Description)
            .WithCustomState(expected, new ErrorReasonComparer());
    }

    [Fact]
    public async Task Validate_When_DescriptionIsValid_Should_Pass()
    {
        //Arrange
        command.UpdateData.Description = "Valid description";

        //Act
        var result = await validator.TestValidateAsync(command.UpdateData);

        //Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    #endregion


    #region PermissionIds Validation

    [Fact]
    public async Task Validate_When_PermissionsEmpty_Should_HaveError()
    {
        //Arrange
        command.UpdateData.PermissionIds = [];
        translator.SetupTranslate(
            RoleErrorMessages.RolePermissionsRequired,
            SharedResource.TranslateText
        );

        //Act
        var result = await validator.TestValidateAsync(command.UpdateData);

        //Assert
        var expected = new ErrorReason(
            RoleErrorMessages.RolePermissionsRequired,
            SharedResource.TranslateText
        );
        result
            .ShouldHaveValidationErrorFor(x => x.PermissionIds)
            .WithCustomState(expected, new ErrorReasonComparer());
    }

    [Fact]
    public async Task Validate_When_PermissionIdsNotUnique_Should_HaveError()
    {
        //Arrange
        var id = Ulid.NewUlid();
        command.UpdateData.PermissionIds = [id, id];

        translator.SetupTranslate(
            RoleErrorMessages.RolePermissionsUnique,
            SharedResource.TranslateText
        );

        //Act
        var result = await validator.TestValidateAsync(command.UpdateData);

        //Assert
        var expected = new ErrorReason(
            RoleErrorMessages.RolePermissionsUnique,
            SharedResource.TranslateText
        );
        result
            .ShouldHaveValidationErrorFor(x => x.PermissionIds)
            .WithCustomState(expected, new ErrorReasonComparer());
    }

    [Fact]
    public async Task Validate_When_PermissionNotExistent_Should_HaveError()
    {
        // Arrange
        var expected = new ErrorReason(
            RoleErrorMessages.RolePermissionsExistent,
            SharedResource.TranslateText
        );

        inlineValidator
            .RuleFor(x => x.UpdateData.PermissionIds)
            .MustAsync((permissionIds, ct) => Task.FromResult(false))
            .When(_ => true, ApplyConditionTo.CurrentValidator)
            .WithState(_ => expected);

        //Act
        var result = await inlineValidator.TestValidateAsync(command);

        //Assert
        result
            .ShouldHaveValidationErrorFor(x => x.UpdateData.PermissionIds)
            .WithCustomState(expected, new ErrorReasonComparer());
    }

    [Fact]
    public async Task Validate_When_AllPermissionIdsAreValid_Should_Pass()
    {
        //Arrange
        inlineValidator
            .RuleFor(x => x.UpdateData.PermissionIds)
            .MustAsync((permissionIds, ct) => Task.FromResult(true))
            .When(_ => true, ApplyConditionTo.CurrentValidator);

        //Act
        var result = await inlineValidator.TestValidateAsync(command);

        //assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateData.PermissionIds);
    }

    #endregion
}
