using Application.Common.ErrorCodes;
using Application.Contracts.Permissions;
using Application.Features.Roles.Commands.Update;
using Shouldly;

namespace Application.SubcutaneousTests.Roles.Commands;

[Collection(nameof(TestingCollectionFixture))]
public class UpdateRoleHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    [Fact]
    public async Task UpdateRole_When_IdNotFound_ShouldReturnNotFoundError()
    {
        //Arrange
        string Id = Ulid.Empty.ToString();
        //Act
        var result = await testingFixture.SendAsync(new UpdateRoleCommand() { RoleId = Id });
        //assert
        var expected = RoleErrorMessages.RoleNotFound;
        result.Error.ShouldNotBeNull();
        result.Error.Status.ShouldBe(404);
        result.Error.ErrorMessage!.Value.Text.ShouldBe(expected);
    }

    [Fact]
    public async Task UpdateRole_ShouldUpdateRoleSuccessfully()
    {
        //Arrange
        var permissions = await testingFixture.SeedingPermissionAsync();
        var role = await testingFixture.CreateManagerRoleAsync();
        UpdateRoleCommand command =
            new()
            {
                RoleId = role.Id.ToString(),
                UpdateData = new RoleUpdateData()
                {
                    Name = role.Name,
                    Description = role.Description,
                    PermissionIds = [.. role.Permissions.Select(x => x.PermissionId)],
                },
            };
        command.UpdateData.Description = "^^";
        command.UpdateData.PermissionIds.RemoveAt(0);
        command.UpdateData.PermissionIds.Add(
            permissions
                .Find(x =>
                    x.Code
                    == PermissionNames.PermissionGenerator.Generate(
                        PermissionNames.PermissionResource.User,
                        PermissionNames.PermissionAction.Update
                    )
                )!
                .Id
        );

        //Act
        var result = await testingFixture.SendAsync(command);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();
        result.Value.ShouldNotBeNull();

        // Directly assert returned response fields
        result.Value.Name.ShouldBe(command.UpdateData.Name);
        result.Value.Description.ShouldBe(command.UpdateData.Description);

        // Permissions must match exactly
        result
            .Value.Permissions!.Select(x => x.Id)
            .ShouldBe(command.UpdateData.PermissionIds, ignoreOrder: true);
    }

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
    }

    public async Task DisposeAsync() => await Task.CompletedTask;
}
