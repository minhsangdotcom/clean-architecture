using Application.Features.Common.Payloads.Users;
using Application.Features.Common.Projections.Users;
using Application.Features.Users.Commands.Create;
using Application.SubcutaneousTests.Extensions;
using AutoFixture;
using Contracts.ApiWrapper;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Infrastructure.Constants;
using Microsoft.AspNetCore.Http;
using SharedKernel.Common.Messages;
using SharedKernel.Constants;
using Shouldly;

namespace Application.SubcutaneousTests.Users.Create;

[Collection(nameof(TestingCollectionFixture))]
public class CreateUserHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    private readonly Fixture fixture = new();
    private Ulid roleId;
    private CreateUserCommand command = new();

    // [Fact]
    // private async Task CreateUser_WhenProvinceNotFound_ShouldReturnNotFoundResult()
    // {
    //     command.ProvinceId = Ulid.NewUlid();
    //     //act
    //     Result<CreateUserResponse> result = await testingFixture.SendAsync(command);

    //     //assert
    //     var expectedMessage = Messenger
    //         .Create<User>()
    //         .Property(nameof(CreateUserCommand.ProvinceId))
    //         .Message(MessageType.Existence)
    //         .Negative()
    //         .Build();

    //     result.Error.ShouldNotBeNull();
    //     result.Error.Status.ShouldBe(404);
    //     result.Error.ErrorMessage.ShouldBe(expectedMessage, new MessageResultComparer());
    // }

    // [Fact]
    // private async Task CreateUser_WhenDistrictNotFound_ShouldReturnNotFoundResult()
    // {
    //     command.DistrictId = Ulid.NewUlid();
    //     //act
    //     Result<CreateUserResponse> result = await testingFixture.SendAsync(command);

    //     //assert
    //     var expectedMessage = Messenger
    //         .Create<User>()
    //         .Property(nameof(CreateUserCommand.DistrictId))
    //         .Message(MessageType.Existence)
    //         .Negative()
    //         .Build();

    //     result.Error.ShouldNotBeNull();
    //     result.Error.Status.ShouldBe(404);
    //     result.Error.ErrorMessage.ShouldBe(expectedMessage, new MessageResultComparer());
    // }

    // [Fact]
    // private async Task CreateUser_WhenCommuneNotFound_ShouldReturnNotFoundResult()
    // {
    //     command.CommuneId = Ulid.NewUlid();
    //     //act
    //     Result<CreateUserResponse> result = await testingFixture.SendAsync(command);

    //     //assert
    //     var expectedMessage = Messenger
    //         .Create<User>()
    //         .Property(nameof(CreateUserCommand.CommuneId))
    //         .Message(MessageType.Existence)
    //         .Negative()
    //         .Build();

    //     result.Error.ShouldNotBeNull();
    //     result.Error.Status.ShouldBe(404);
    //     result.Error.ErrorMessage.ShouldBe(expectedMessage, new MessageResultComparer());
    // }

    [Fact]
    private async Task CreateUser_ShouldCreateSuccess()
    {
        //arrage
        command.DateOfBirth = null;
        command.Avatar = null;
        command.Gender = null;
        command.UserClaims = null;

        //act
        Result<CreateUserResponse> result = await testingFixture.SendAsync(command);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();

        var response = result.Value!;
        var user = await testingFixture.FindUserByIdAsync(response.Id);
        user.ShouldNotBeNull();

        user!.ShouldSatisfyAllConditions(
            () => user.Id.ShouldBe(response.Id),
            () => user.FirstName.ShouldBe(response.FirstName),
            () => user.LastName.ShouldBe(response.LastName),
            () => user.Username.ShouldBe(response.Username),
            () => user.Email.ShouldBe(response.Email),
            () => user.PhoneNumber.ShouldBe(response.PhoneNumber),
            () => user.DateOfBirth.ShouldBe(response.DayOfBirth),
            () => user.Gender.ShouldBe(response.Gender),
            () => user.Avatar.ShouldBe(response.Avatar),
            () => user.Status.ShouldBe(response.Status),
            () => user.Roles?.Select(x => x.RoleId).ShouldBe(response.Roles?.Select(x => x.Id)),
            () =>
                command
                    .UserClaims?.All(x =>
                        user.Claims?.Any(p =>
                            p.ClaimType == x.ClaimType && p.ClaimValue == x.ClaimType
                        ) == true
                    )
                    .ShouldBeTrue()
        );
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
        UserAddress address = await testingFixture.SeedingRegionsAsync();
        Role role = await testingFixture.CreateAdminRoleAsync();
        roleId = role.Id;

        IFormFile file = FileHelper.GenerateIFormfile(
            Path.Combine(Directory.GetCurrentDirectory(), "Files", "avatar_cute_2.jpg")
        );
        command = fixture
            .Build<CreateUserCommand>()
            .With(x => x.Avatar, file)
            .With(
                x => x.UserClaims,
                [
                    .. Credential
                        .MANAGER_CLAIMS.Select(x => new UserClaimPayload()
                        {
                            ClaimType = ClaimTypes.Permission,
                            ClaimValue = x,
                        })
                        .ToList(),
                ]
            )
            .With(x => x.Roles, [roleId])
            .With(x => x.Email, "admin@gmail.com")
            .With(x => x.PhoneNumber, "0123456789")
            .With(x => x.Username, "admin.super")
            .Create();
    }
}
