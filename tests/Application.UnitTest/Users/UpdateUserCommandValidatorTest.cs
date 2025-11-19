using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections.Users;
using Application.Features.Common.Requests.Users;
using Application.Features.Users.Commands.Update;
using AutoFixture;
using Domain.Aggregates.Users;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using SharedKernel.Common.Messages;

namespace Application.UnitTest.Users;

public class UpdateUserCommandValidatorTest
{
    private readonly UserUpdateRequest userUpdate;
    private readonly Fixture fixture = new();

    private readonly UpdateUserCommandValidator validator;
    private readonly InlineValidator<UserUpdateRequest> mockValidator = [];

    public UpdateUserCommandValidatorTest()
    {
        Mock<IEfUnitOfWork> mockUserManagerService = new();
        Mock<IHttpContextAccessorService> mockHttpContextAccessorService = new();
        Mock<ICurrentUser> currentUserService = new();
        // validator = new(
        //     mockUserManagerService.Object,
        //     mockHttpContextAccessorService.Object,
        //     currentUserService.Object
        // );
        // userUpdate = fixture
        //     .Build<UserUpdateRequest>()
        //     .With(x => x.ProvinceId, Ulid.Parse("01JRQHWS3RQR1N0J84EV1DQXR1"))
        //     .With(x => x.DistrictId, Ulid.Parse("01JRQHWSNPR3Z8Z20GBSB22CSJ"))
        //     .With(x => x.CommuneId, Ulid.Parse("01JRQHWTCHN5WBZ12WC08AZCZ8"))
        //     .Without(x => x.Avatar)
        //     .With(
        //         x => x.UserClaims,
        //         [new UserClaimUpsertCommand() { ClaimType = "test", ClaimValue = "test.value" }]
        //     )
        //     .With(x => x.Roles, [Ulid.Parse("01JS72XZJ6NFKFVWA9QM03RY5G")])
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
        userUpdate!.FirstName = firstName;

        //act
        var result = await validator.TestValidateAsync(userUpdate);

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
        userUpdate!.FirstName = new string([.. fixture.CreateMany<char>(257)]);

        //act
        var result = await validator.TestValidateAsync(userUpdate);

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
        userUpdate!.LastName = lastName;
        //act
        var result = await validator.TestValidateAsync(userUpdate);

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
        userUpdate!.LastName = new string([.. fixture.CreateMany<char>(257)]);

        //act
        var result = await validator.TestValidateAsync(userUpdate);

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
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WhenPhoneNumberNullOrEmpty_ShouldReturNullFailure(string phoneNumber)
    {
        userUpdate.PhoneNumber = phoneNumber;

        //act
        var result = await validator.TestValidateAsync(userUpdate);

        //assert
        var expectedState = Messenger
            .Create<User>()
            .Property(x => x.PhoneNumber!)
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
        userUpdate.PhoneNumber = phoneNumber;

        //act
        var result = await validator.TestValidateAsync(userUpdate);

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
    public async Task Validate_WhenProvinceEmpty_ShouldReturnNullFailure()
    {
        // userUpdate!.ProvinceId = Ulid.Empty;

        // //act
        // var result = await validator.TestValidateAsync(userUpdate);

        // //assert
        // var expectedState = Messenger
        //     .Create<User>()
        //     .Property(nameof(UserUpdateRequest.ProvinceId))
        //     .Message(MessageType.Null)
        //     .Negative()
        //     .Build();
        // result
        //     .ShouldHaveValidationErrorFor(x => x.ProvinceId)
        //     .WithCustomState(expectedState, new MessageResultComparer())
        //     .Only();
    }

    [Fact]
    public async Task Validate_WhenDistrictEmpty_ShouldReturnNullFailure()
    {
        // userUpdate!.DistrictId = Ulid.Empty;
        // //act
        // var result = await validator.TestValidateAsync(userUpdate);

        // //assert
        // var expectedState = Messenger
        //     .Create<User>()
        //     .Property(nameof(UserUpdateRequest.DistrictId))
        //     .Message(MessageType.Null)
        //     .Negative()
        //     .Build();
        // result
        //     .ShouldHaveValidationErrorFor(x => x.DistrictId)
        //     .WithCustomState(expectedState, new MessageResultComparer())
        //     .Only();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_WhenStreetNullOrEmpty_ShouldReturnNullFailure(string? street)
    {
        // userUpdate!.Street = street;

        // //act
        // var result = await validator.TestValidateAsync(userUpdate);

        // //assert
        // var expectedState = Messenger
        //     .Create<User>()
        //     .Property(nameof(UserUpdateRequest.Street))
        //     .Message(MessageType.Null)
        //     .Negative()
        //     .Build();
        // result
        //     .ShouldHaveValidationErrorFor(x => x.Street)
        //     .WithCustomState(expectedState, new MessageResultComparer())
        //     .Only();
    }

    [Fact]
    public async Task Validate_WhenRolesNull_ShouldReturnNullFailure()
    {
        userUpdate!.Roles = null;

        //act
        var result = await validator.TestValidateAsync(userUpdate);
        //assert
        var expectedState = Messenger
            .Create<UserUpdateRequest>(nameof(User))
            .Property(x => x.Roles!)
            .Message(MessageType.Null)
            .Negative()
            .Build();
        result
            .ShouldHaveValidationErrorFor(x => x.Roles)
            .WithCustomState(expectedState, new MessageResultComparer())
            .Only();
    }
}
