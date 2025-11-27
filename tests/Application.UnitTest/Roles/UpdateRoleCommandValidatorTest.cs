using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Messages;
using Application.Features.Roles.Commands.Update;
using Application.SharedFeatures.Requests.Roles;
using AutoFixture;
using Domain.Aggregates.Roles;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;

namespace Application.UnitTest.Roles;

public sealed class UpdateRoleCommandValidatorTest
{
    private readonly InlineValidator<UpdateRoleRequest> mockValidator;
    private readonly UpdateRoleCommandValidator validator;

    private readonly UpdateRoleRequest command;
    private readonly Fixture fixture = new();
    private readonly Mock<IEfUnitOfWork> unitOfWork = new();
    private readonly Mock<IMessageTranslatorService> translator = new();
    private readonly Mock<IHttpContextAccessorService> mockHttpContextAccessorService = new();

    public UpdateRoleCommandValidatorTest()
    {
        mockValidator = [];
        validator = new UpdateRoleCommandValidator(
            unitOfWork.Object,
            mockHttpContextAccessorService.Object,
            translator.Object
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_NameIsNullOrEmpty_ShouldHaveNotEmptyFailure(string? name)
    {
        // arrage
        command.Name = name;

        // act
        var result = await validator.TestValidateAsync(command);

        //assert
        string errorMessage = Messenger
            .Create<RoleUpsertCommand>(nameof(Role))
            .Property(x => x.Name!)
            .Negative()
            .WithError(MessageErrorType.Required)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Object.Translate(errorMessage));

        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_NameTooLong_ShouldHaveMaximumLengthFailure()
    {
        // arrage
        command.Name = new string([.. fixture.CreateMany<char>(257)]);

        // act
        var result = await validator.TestValidateAsync(command);

        //assert
        string errorMessage = Messenger
            .Create<RoleUpsertCommand>(nameof(Role))
            .Property(x => x.Name!)
            .Negative()
            .WithError(MessageErrorType.Required)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Object.Translate(errorMessage));

        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_WhenNameExists_ShouldHaveExistenceFailure()
    {
        const string existedName = "ADMIN";
        command.Name = existedName;

        string errorMessage = Messenger
            .Create<RoleUpsertCommand>(nameof(Role))
            .Property(x => x.Name!)
            .WithError(MessageErrorType.Existent)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Object.Translate(errorMessage));

        mockValidator
            .RuleFor(x => x.Name)
            .Must(x => x != existedName)
            .WithState(_ => expectedState);

        var result = await mockValidator.TestValidateAsync(command);

        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_DescriptionTooLong_ShouldHaveMaximumLengthFailure()
    {
        command.Description = new string([.. fixture.CreateMany<char>(10001)]);

        var result = await validator.TestValidateAsync(command);

        string errorMessage = Messenger
            .Create<RoleUpsertCommand>(nameof(Role))
            .Property(x => x.Description!)
            .WithError(MessageErrorType.TooLong)
            .GetFullMessage();

        ErrorReason expectedState = new(errorMessage, translator.Object.Translate(errorMessage));

        result
            .ShouldHaveValidationErrorFor(x => x.Description)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }
}
