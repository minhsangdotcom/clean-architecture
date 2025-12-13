using Application.Features.Roles.Commands.Create;
using Domain.Aggregates.Roles;
using Shouldly;

namespace Application.SubcutaneousTests.Roles.Commands;

[Collection(nameof(TestingCollectionFixture))]
public class CreateRoleHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    [Fact]
    public async Task CreateRole_ShouldCreateSuccessfully()
    {
        //Arrange
        var permissions = await testingFixture.SeedingPermissionAsync();
        CreateRoleCommand command =
            new() { Name = "test", PermissionIds = [.. permissions.Take(2).Select(x => x.Id)] };

        // Act
        var result = await testingFixture.SendAsync(command);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        Role? role = await testingFixture.FindRoleByIdIncludeChildrenAsync(result.Value.Id);
        role.ShouldNotBeNull();

        // Validate role fields
        role.Name.ShouldBe(command.Name);
        role.Id.ShouldBe(result.Value.Id);

        // Permissions must match exactly what was sent
        var createdPermissionIds = role.Permissions.Select(p => p.PermissionId).ToList();
        createdPermissionIds.Count.ShouldBe(command.PermissionIds.Count);
        createdPermissionIds.ShouldBe(command.PermissionIds, ignoreOrder: true);
    }

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
    }

    public async Task DisposeAsync() => await Task.CompletedTask;
}
