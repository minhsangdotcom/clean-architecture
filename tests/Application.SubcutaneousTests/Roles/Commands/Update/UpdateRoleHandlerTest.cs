namespace Application.SubcutaneousTests.Roles.Commands.Update;

[Collection(nameof(TestingCollectionFixture))]
public class UpdateRoleHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    [Fact]
    public async Task UpdateRole_When_IdNotFound_ShouldReturnNotFoundError() { }

    [Fact]
    public async Task UpdateRole_When_NoDescription_ShouldUpdateRoleSuccessfully() { }

    [Fact]
    public async Task UpdateRole_ShouldUpdateRoleSuccessfully() { }

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
    }

    public async Task DisposeAsync() => await Task.CompletedTask;
}
