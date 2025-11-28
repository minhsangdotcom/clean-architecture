using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Repositories.EfCore;
using Application.Common.Interfaces.Services;
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

    private readonly Mock<IHttpContextAccessorService> mockHttpContextAccessorService = new();
    private readonly Mock<IMessageTranslatorService> translator = new();

    public UpdateRoleCommandValidatorTest()
    {
        mockHttpContextAccessorService
            .Setup(x => x.GetHttpMethod())
            .Returns(HttpMethod.Put.ToString());

        Mock<IAsyncRepository<Role>> roleRepo = new();
        Mock<IAsyncRepository<Permission>> permissionRepo = new();

        unitOfWork.Setup(x => x.Repository<Role>()).Returns(roleRepo.Object);
        unitOfWork.Setup(x => x.Repository<Permission>()).Returns(permissionRepo.Object);

        validator = new(
            unitOfWork.Object,
            mockHttpContextAccessorService.Object,
            translator.Object
        );
        inlineValidator = [];
        ResetCommand();
    }

    private void ResetCommand()
    {
        var updateDataFaker = new Faker<UpdateRoleRequest>()
            .RuleFor(x => x.Name, f => f.Commerce.Department())
            .RuleFor(x => x.Description, f => f.Lorem.Sentence(8))
            .RuleFor(x => x.PermissionIds, f => [Ulid.NewUlid(), Ulid.NewUlid(), Ulid.NewUlid()]);
        var faker = new Faker<UpdateRoleCommand>().RuleFor(
            x => x.UpdateData,
            f => updateDataFaker.Generate()
        );

        command = faker.Generate();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Should_HaveError_When_NameIsNullOrEmpty(string? name)
    {
        //Arrange
        command.UpdateData.Name = name;
        translator.SetupTranslate(RoleErrorMessages.RoleNameRequired, SharedResource.TranslateText);

        //Act
        var result = await validator.TestValidateAsync(command);

        //Assert
        var expected = new ErrorReason(
            RoleErrorMessages.RoleNameRequired,
            SharedResource.TranslateText
        );
        result
            .ShouldHaveValidationErrorFor(x => x.UpdateData.Name)
            .WithCustomState(expected, new ErrorReasonComparer());
    }

    [Fact]
    public async Task Should_HaveError_When_NameTooLong()
    {
        command.UpdateData.Name = new string('X', 300);
        translator.SetupTranslate(RoleErrorMessages.RoleNameTooLong, SharedResource.TranslateText);

        var result = await validator.TestValidateAsync(command);
        var expected = new ErrorReason(
            RoleErrorMessages.RoleNameTooLong,
            SharedResource.TranslateText
        );

        result
            .ShouldHaveValidationErrorFor(x => x.UpdateData.Name)
            .WithCustomState(expected, new ErrorReasonComparer());
    }

    [Fact]
    public async Task Should_HaveError_When_NameAlreadyExists()
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
    public async Task Should_Pass_When_NameIsUnique()
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

    [Fact]
    public async Task Should_HaveError_When_DescriptionTooLong()
    {
        //Arrange
        command.UpdateData.Description = new string('D', 2000);
        translator.SetupTranslate(
            RoleErrorMessages.RoleDescriptionTooLong,
            SharedResource.TranslateText
        );

        //Act
        var result = await validator.TestValidateAsync(command);

        //Assert
        var expected = new ErrorReason(
            RoleErrorMessages.RoleDescriptionTooLong,
            SharedResource.TranslateText
        );
        result
            .ShouldHaveValidationErrorFor(x => x.UpdateData.Description)
            .WithCustomState(expected, new ErrorReasonComparer());
    }

    [Fact]
    public async Task Should_Pass_When_DescriptionIsValid()
    {
        //Arrange
        command.UpdateData.Description = "Valid description";

        //Act
        var result = await validator.TestValidateAsync(command);

        //Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateData.Description);
    }

    [Fact]
    public async Task Should_HaveError_When_PermissionsEmpty()
    {
        //Arrange
        command.UpdateData.PermissionIds = [];
        translator.SetupTranslate(
            RoleErrorMessages.RolePermissionsRequired,
            SharedResource.TranslateText
        );

        //Act
        var result = await validator.TestValidateAsync(command);

        //Assert
        var expected = new ErrorReason(
            RoleErrorMessages.RolePermissionsRequired,
            SharedResource.TranslateText
        );
        result
            .ShouldHaveValidationErrorFor(x => x.UpdateData.PermissionIds)
            .WithCustomState(expected, new ErrorReasonComparer());
    }

    [Fact]
    public async Task Should_HaveError_When_PermissionIdsNotUnique()
    {
        //Arrange
        var id = Ulid.NewUlid();
        command.UpdateData.PermissionIds = [id, id];

        translator.SetupTranslate(
            RoleErrorMessages.RolePermissionsUnique,
            SharedResource.TranslateText
        );

        //Act
        var result = await validator.TestValidateAsync(command);

        //Assert
        var expected = new ErrorReason(
            RoleErrorMessages.RolePermissionsUnique,
            SharedResource.TranslateText
        );
        result
            .ShouldHaveValidationErrorFor(x => x.UpdateData.PermissionIds)
            .WithCustomState(expected, new ErrorReasonComparer());
    }

    [Fact]
    public async Task Should_HaveError_When_PermissionNotExistent()
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
    public async Task Should_Pass_When_AllPermissionIdsAreValid()
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
}
