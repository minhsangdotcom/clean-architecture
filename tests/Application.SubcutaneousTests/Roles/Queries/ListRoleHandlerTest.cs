using Application.Features.Roles.Queries.List;
using Shouldly;

namespace Application.SubcutaneousTests.Roles.Queries;

[Collection(nameof(TestingCollectionFixture))]
public class ListRoleHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    [Fact]
    public async Task ListRole_When_BeEmpty_ShouldReturnEmptyList()
    {
        //Act
        var result = await testingFixture.SendAsync(new ListRoleQuery());

        //Assert
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeNull();
        result.Value.ShouldNotBeNull();

        result.Value.ShouldBe([]);
    }

    [Fact]
    public async Task ListRole_ShouldReturnListRole()
    {
        //Arrange
        _ = await testingFixture.SeedingPermissionAsync();
        var role = await testingFixture.CreateAdminRoleAsync();

        //Act
        var result = await testingFixture.SendAsync(new ListRoleQuery());

        //Assert
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeNull();
        result.Value.ShouldNotBeNull();

        result.Value.Count.ShouldBe(1);
        var firstRole = result.Value[0];

        firstRole.ShouldNotBeNull();
        firstRole.Id.ShouldBe(role.Id);
        firstRole.Name.ShouldBe(role.Name);
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
    }
}
