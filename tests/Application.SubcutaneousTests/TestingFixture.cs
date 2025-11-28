using Application.SubcutaneousTests.Extensions;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Application.SubcutaneousTests;

public partial class TestingFixture : IAsyncLifetime
{
    private CustomWebApplicationFactory<Program>? factory;
    private readonly PostgreSqlDatabase database;

    private const string BASE_URL = "http://localhost:8080/api/v1";

    private HttpClient? client;
    private static Ulid UserId;

    public TestingFixture()
    {
        database = new();
    }

    public async Task DisposeAsync()
    {
        await database.DisposeAsync();
        if (factory != null)
        {
            await factory!.DisposeAsync();
        }

        if (client != null)
        {
            client!.Dispose();
        }
    }

    public async Task InitializeAsync()
    {
        await database.InitializeAsync();
        var connection = database.GetConnection();
        string environmentName = database.GetEnvironmentVariable();
        factory = new(connection, environmentName);
        CreateClient();
    }

    public async Task ResetAsync()
    {
        if (database != null)
        {
            await database.ResetAsync();
        }
    }

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        factory.ThrowIfNull();
        using var scope = factory!.Services.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(request);
    }

    private void CreateClient()
    {
        factory.ThrowIfNull();
        client = factory!.CreateClient();
    }

    public static Ulid GetUserId() => UserId;

    public static void RemoveUserId() => UserId = Ulid.Empty;
}
