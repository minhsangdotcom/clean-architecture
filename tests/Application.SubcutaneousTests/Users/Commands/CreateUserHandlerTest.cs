using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.Create;
using Application.SubcutaneousTests.Extensions;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Microsoft.AspNetCore.Http;
using Shouldly;

namespace Application.SubcutaneousTests.Users.Commands;

[Collection(nameof(TestingCollectionFixture))]
public class CreateUserHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    private CreateUserCommand command = null!;

    [Fact]
    public async Task CreateUser_ShouldCreateSuccess()
    {
        //Act
        Result<CreateUserResponse> result = await testingFixture.SendAsync(command);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();

        CreateUserResponse response = result.Value!;
        User? user = await testingFixture.FindUserByIdInCludeChildrenAsync(response.Id);
        user.ShouldNotBeNull();

        user.Username.ShouldBe(command.Username);
        user.FirstName.ShouldBe(command.FirstName);
        user.LastName.ShouldBe(command.LastName);
        user.Email.ShouldBe(command.Email);
        user.PhoneNumber.ShouldBe(command.PhoneNumber);
        user.Gender.ShouldBe(command.Gender);
        user.Status.ShouldBe(command.Status);

        user.DateOfBirth.ShouldBe(command.DateOfBirth!.Value);
        user.Roles.Select(x => x.RoleId).ShouldBe(command.Roles);
        user.Permissions.Select(x => x.PermissionId).ShouldBe(command.Permissions, true);

        user.Avatar.ShouldNotBeNull();
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
        List<Permission> permissions = await testingFixture.SeedingPermissionAsync();
        Role role = await testingFixture.CreateAdminRoleAsync();

        IFormFile file = FileHelper.GenerateIFormFile(
            Path.Combine(Directory.GetCurrentDirectory(), "Files", "avatar_cute_2.jpg")
        );
        command = new()
        {
            Username = "john.doe",
            Password = "StrongPassword123!",
            Gender = Gender.Male,

            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+84901234567",
            Email = "john.doe@example.com",
            DateOfBirth = new DateTime(1995, 5, 21),

            Avatar = file,
            Status = UserStatus.Active,

            Roles = [role.Id],
            Permissions = [.. permissions.Take(2).Select(x => x.Id)],
        };
    }
}
