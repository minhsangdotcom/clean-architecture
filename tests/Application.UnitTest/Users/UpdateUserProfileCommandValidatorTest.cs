using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Users.Commands.Profiles;
using AutoFixture;
using Domain.Aggregates.Users;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using SharedKernel.Common.Messages;

namespace Application.UnitTest.Users;

public class UpdateUserProfileCommandValidatorTest
{
    private readonly UpdateUserProfileCommand command;
    private readonly Fixture fixture = new();

    private readonly UpdateUserProfileCommandValidator validator;
    private readonly InlineValidator<UpdateUserProfileCommand> mockValidator = [];

    public UpdateUserProfileCommandValidatorTest()
    {
        Mock<IEfUnitOfWork> mockUserManagerService = new();
        Mock<IHttpContextAccessorService> mockHttpContextAccessorService = new();
        Mock<ICurrentUser> currentUserService = new();
        validator = new(
            mockUserManagerService.Object,
            mockHttpContextAccessorService.Object,
            currentUserService.Object
        );
        // command = fixture
        //     .Build<UpdateUserProfileCommand>()
        //     .With(x => x.ProvinceId, Ulid.Parse("01JRQHWS3RQR1N0J84EV1DQXR1"))
        //     .With(x => x.DistrictId, Ulid.Parse("01JRQHWSNPR3Z8Z20GBSB22CSJ"))
        //     .With(x => x.CommuneId, Ulid.Parse("01JRQHWTCHN5WBZ12WC08AZCZ8"))
        //     .Without(x => x.Avatar)
        //     .With(x => x.Email, "admin@gmail.com")
        //     .With(x => x.PhoneNumber, "0123456789")
        //     .Create();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenFirstNameNullOrEmpty_ShouldReturnNullFailure(string? firstName)
    {
        //arrage
        command!.FirstName = firstName;

        //act
        var result = await validator.TestValidateAsync(command);

        //assert
        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.FirstName)
            .Message(MessageType.Null)
            .Negative()
            .Build();
        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithCustomState(expectedState, new MessageResultComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_WhenInvalidLengthOfFirstName_ShouldReturnMaximumLengthFailure()
    {
        command!.FirstName = new string([.. fixture.CreateMany<char>(257)]);

        //act
        var result = await validator.TestValidateAsync(command);

        //assert
        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.FirstName)
            .Message(MessageType.MaximumLength)
            .Build();
        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithCustomState(expectedState, new MessageResultComparer())
            .Only();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenLastNameNullOrEmpty_ShouldReturnNullFailure(string? lastName)
    {
        command!.LastName = lastName;
        //act
        var result = await validator.TestValidateAsync(command);

        //assert
        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.LastName)
            .Message(MessageType.Null)
            .Negative()
            .Build();
        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithCustomState(expectedState, new MessageResultComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_WhenInvalidLengthOfLastName_ShouldReturnMaximumLengthFailure()
    {
        command!.LastName = new string([.. fixture.CreateMany<char>(257)]);

        //act
        var result = await validator.TestValidateAsync(command);

        //assert
        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.LastName)
            .Message(MessageType.MaximumLength)
            .Build();
        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithCustomState(expectedState, new MessageResultComparer())
            .Only();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_WhenEmailNullOrEmpty_ShouldReturnNullFailure(string? email)
    {
        command!.Email = email;

        //act
        var result = await validator.TestValidateAsync(command);

        //assert
        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.Email)
            .Message(MessageType.Null)
            .Negative()
            .Build();
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithCustomState(expectedState, new MessageResultComparer())
            .Only();
    }

    [Theory]
    [InlineData("admin@gmail")]
    [InlineData("admingmail.com")]
    [InlineData("@gmail.com")]
    public async Task CreateUser_WhenEmailInvalidFormat_ShouldReturnInvalidFailure(string email)
    {
        command!.Email = email;

        //act
        var result = await validator.TestValidateAsync(command);

        //assert
        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.Email)
            .Message(MessageType.Valid)
            .Negative()
            .Build();
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithCustomState(expectedState, new MessageResultComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_WhenEmailDuplicated_ShouldReturnExistFailure()
    {
        const string existedEmail = "admin@gmail.com";
        command!.Email = existedEmail;
        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.Email)
            .Message(MessageType.Existence)
            .Build();

        mockValidator
            .RuleFor(x => x.Email)
            .MustAsync(
                (email, cancellationToken) =>
                    IsEmailAvailableAsync(email!, existedEmail, cancellationToken)
            )
            .When(_ => true)
            .WithState(x => expectedState);

        //act
        var result = await mockValidator.TestValidateAsync(command);

        //assert
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithCustomState(expectedState, new MessageResultComparer())
            .Only();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenPhoneNumberNullOrEmpty_ShouldReturNullFailure(string phoneNumber)
    {
        command.PhoneNumber = phoneNumber;

        //act
        var result = await validator.TestValidateAsync(command);

        //assert
        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.PhoneNumber)
            .Message(MessageType.Null)
            .Negative()
            .Build();
        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithCustomState(expectedState, new MessageResultComparer())
            .Only();
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("++12345678")]
    public async Task Validate_WhenPhoneNumberInvalidFormat_ShouldReturnInvalidFailure(
        string phoneNumber
    )
    {
        command.PhoneNumber = phoneNumber;

        //act
        var result = await validator.TestValidateAsync(command);

        //assert
        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.PhoneNumber)
            .Message(MessageType.Valid)
            .Negative()
            .Build();

        result
            .ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithCustomState(expectedState, new MessageResultComparer())
            .Only();
    }

    [Fact]
    public async Task Validate_WhenProvinceEmpty_ShouldReturnNullFailure() { }

    [Fact]
    public async Task Validate_WhenDistrictEmpty_ShouldReturnNullFailure() { }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_WhenStreetNullOrEmpty_ShouldReturnNullFailure(string? street) { }

    private static async Task<bool> IsEmailAvailableAsync(
        string email,
        string existedEmail,
        CancellationToken cancellationToken = default
    )
    {
        return await Task.Run(() => email != existedEmail, cancellationToken);
    }
}
