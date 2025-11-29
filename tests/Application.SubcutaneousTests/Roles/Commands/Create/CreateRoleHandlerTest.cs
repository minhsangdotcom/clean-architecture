namespace Application.SubcutaneousTests.Roles.Commands.Create;

[Collection(nameof(TestingCollectionFixture))]
public class CreateRoleHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    [Fact]
    public async Task CreateRole_When_NoDescription_ShouldCreateSuccessfully() { }

    [Fact]
    public async Task CreateRole_ShouldCreateSuccessfully() { }

    public async Task InitializeAsync()
    {
        await testingFixture.ResetAsync();
    }

    public async Task DisposeAsync() => await Task.CompletedTask;
}
