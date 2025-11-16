using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections.Roles;
using Application.Features.Common.Requests.Roles;
using Application.Features.Roles.Commands.Update;
using AutoFixture;
using Domain.Aggregates.Roles;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using SharedKernel.Common.Messages;

namespace Application.UnitTest.Roles;

public sealed class UpdateRoleCommandValidatorTest
{
    private readonly InlineValidator<UpdateRoleRequest> mockValidator;
    private readonly UpdateRoleCommandValidator validator;

    private readonly UpdateRoleRequest command;
    private readonly List<RoleClaimUpsertCommand> roleClaims;
    private readonly Fixture fixture = new();
    private readonly Mock<IEfUnitOfWork> mockRoleManager = new();
    private readonly Mock<IHttpContextAccessorService> mockHttpContextAccessorService = new();

    public UpdateRoleCommandValidatorTest()
    {
        mockValidator = [];
        validator = new UpdateRoleCommandValidator(
            mockRoleManager.Object,
            mockHttpContextAccessorService.Object
        );
        roleClaims = [.. fixture.Build<RoleClaimUpsertCommand>().Without(x => x.Id).CreateMany(2)];
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
        MessageResult expectedState = Messenger
            .Create<RoleUpsertCommand>(nameof(Role))
            .Property(x => x.Name!)
            .Negative()
            .Message(MessageType.Null)
            .Build();

        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithCustomState(expectedState, new MessageResultComparer())
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
        MessageResult expectedState = Messenger
            .Create<RoleUpsertCommand>(nameof(Role))
            .Property(x => x.Name!)
            .Message(MessageType.MaximumLength)
            .Build();

        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithCustomState(expectedState, new MessageResultComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_WhenNameExists_ShouldHaveExistenceFailure()
    {
        //arrage
        const string existedName = "ADMIN";
        command.Name = existedName;
        MessageResult expectedState = Messenger
            .Create<RoleUpsertCommand>(nameof(Role))
            .Property(x => x.Name!)
            .Message(MessageType.Existence)
            .Build();

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
            .WithCustomState(expectedState, new MessageResultComparer())
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
        MessageResult expectedState = Messenger
            .Create<RoleUpsertCommand>(nameof(Role))
            .Property(x => x.Description!)
            .Message(MessageType.MaximumLength)
            .Build();

        result
            .ShouldHaveValidationErrorFor(x => x.Description)
            .WithCustomState(expectedState, new MessageResultComparer())
            .Only();
    }
}
