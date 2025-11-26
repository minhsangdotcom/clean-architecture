using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Localization;
using Application.Contracts.Messages;
using Application.Features.Roles.Commands.Create;
using Application.SharedFeatures.Requests.Roles;
using AutoFixture;
using Domain.Aggregates.Roles;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
using Moq;

namespace Application.UnitTest.Roles;

public class CreateRoleCommandValidatorTest
{
    private readonly InlineValidator<CreateRoleCommand> mockValidator;
    private readonly CreateRoleCommandValidator validator;

    private readonly CreateRoleCommand command;
    private readonly Fixture fixture = new();
    private readonly Mock<IEfUnitOfWork> unitOfWork = new();
    private readonly Mock<IHttpContextAccessorService> mockHttpContextAccessorService = new();
    private readonly Mock<IMessageTranslatorService> translator = new();

    public CreateRoleCommandValidatorTest()
    {
        mockValidator = [];
        validator = new CreateRoleCommandValidator(
            unitOfWork.Object,
            mockHttpContextAccessorService.Object,
            translator.Object
        );
        command = new();
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
            .WithError(MessageErrorType.TooLong)
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
        //arrage
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
            .Must(name => command.Name != existedName)
            .When(_ => true)
            .WithState(x => expectedState);

        // act
        var result = await mockValidator.TestValidateAsync(command);
        //assert
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithCustomState(expectedState, new ErrorReasonComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_DescriptionTooLong_ShouldHaveMaximumLengthFailure()
    {
        //arrage
        command.Description = new string([.. fixture.CreateMany<char>(10001)]);

        //act
        var result = await validator.TestValidateAsync(command);

        //assert
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
