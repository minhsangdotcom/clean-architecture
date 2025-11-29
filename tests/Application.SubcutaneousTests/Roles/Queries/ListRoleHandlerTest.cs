using Application.Features.Roles.Queries.List;
using Shouldly;

namespace Application.SubcutaneousTests.Roles.Queries;

[Collection(nameof(TestingCollectionFixture))]
public class ListRoleHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    [Fact]
    public async Task ListRole_When_BeEmpty_ShouldReturnEmptyList()
    {
        var result = await testingFixture.SendAsync(new ListRoleQuery());

        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeNull();

        result.Value.ShouldBe([]);
    }

    [Fact]
    public async Task ListRole_ShouldReturnListRole()
    {
        var role = await testingFixture.CreateAdminRoleAsync();
        var result = await testingFixture.SendAsync(new ListRoleQuery());

        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeNull();

        result.Value?.Count().ShouldBe(1);
        var firstRole = result.Value?.ElementAt(0);

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
