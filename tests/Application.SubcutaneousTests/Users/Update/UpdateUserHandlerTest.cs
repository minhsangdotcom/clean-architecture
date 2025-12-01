using Application.Common.ErrorCodes;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.Update;
using Application.SubcutaneousTests.Extensions;
using Microsoft.AspNetCore.Http;
using Shouldly;

namespace Application.SubcutaneousTests.Users.Update;

[Collection(nameof(TestingCollectionFixture))]
public class UpdateUserHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    private UpdateUserCommand command = new();
    private readonly List<Ulid> additionalPermission = [];

    [Fact]
    public async Task UpdateUser_WhenIdNotfound_ShouldReturnNotFoundResult()
    {
        //Arrange
        command.UserId = Ulid.NewUlid().ToString();

        //Act
        Result<UpdateUserResponse> result = await testingFixture.SendAsync(command);

        //Assert
        var expected = UserErrorMessages.UserNotFound;
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(404);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(expected);
    }

    [Fact]
    public async Task UpdateUser_ShouldUpdateSuccess()
    {
        //Arrange
        var role = await testingFixture.CreateNormalRoleAsync();
        var updateData = command.UpdateData;
        updateData.DateOfBirth = null;
        updateData.Avatar = null;
        updateData.Roles!.RemoveAt(0);
        updateData.Roles.Add(role.Id);
        updateData.Permissions!.RemoveAt(1);
        updateData.Permissions.Add(additionalPermission[^1]);

        //act
        Result<UpdateUserResponse> result = await testingFixture.SendAsync(command);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();

        var response = result.Value!;
        var user = await testingFixture.FindUserByIdInCludeChildrenAsync(response.Id);
        user.ShouldNotBeNull();

        //Assert
        user.FirstName.ShouldBe(updateData.FirstName);
        user.LastName.ShouldBe(updateData.LastName);
        user.Email.ShouldBe(updateData.Email);
        user.PhoneNumber.ShouldBe(updateData.PhoneNumber);
        user.Status.ShouldBe(updateData.Status);

        user.DateOfBirth.ShouldBeNull();
        user.Avatar.ShouldBeNull();
        user.Roles.Select(r => r.RoleId).ShouldBe(updateData.Roles);
        user.Permissions.Select(p => p.PermissionId).ShouldBe(updateData.Permissions);
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
        var permissions = await testingFixture.SeedingPermissionAsync();
        additionalPermission.AddRange(permissions.Select(p => p.Id));

        IFormFile file = FileHelper.GenerateIFormFile(
            Path.Combine(Directory.GetCurrentDirectory(), "Files", "avatar_cute_2.jpg")
        );
        var user = await testingFixture.CreateManagerUserAsync(
            file,
            permissionId: [.. additionalPermission.Take(2)]
        );
        command = UserMappingExtension.ToUpdateUserCommand(user);
    }
}
