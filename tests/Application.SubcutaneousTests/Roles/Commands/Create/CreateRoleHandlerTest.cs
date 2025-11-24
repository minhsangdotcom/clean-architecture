using AutoFixture;

namespace Application.SubcutaneousTests.Roles.Commands.Create;

[Collection(nameof(TestingCollectionFixture))]
public class CreateRoleHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    private readonly Fixture fixture = new();

    [Fact]
    public async Task CreateRole_WhenNoRoleClaims_ShouldCreateRole() { }

    [Fact]
    public async Task CreateRole_WhenNoDescription_ShouldCreateRole() { }

    [Fact]
    public async Task CreateRole_ShouldCreateRole() { }

    public async Task InitializeAsync() => await testingFixture.ResetAsync();

    public async Task DisposeAsync() => await Task.CompletedTask;
}
