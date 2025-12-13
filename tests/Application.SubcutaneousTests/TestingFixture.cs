using Application.SubcutaneousTests.Extensions;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Application.SubcutaneousTests;

public partial class TestingFixture : IAsyncLifetime
{
    private CustomWebApplicationFactory<Program>? factory;
    private readonly PostgreSqlDatabase database = new();

    private const string BASE_URL = "http://localhost:8080/api/v1";

    private HttpClient? client;
    private static Ulid UserId;

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
        var connection = database.Connection;
        string environmentName = database.EnvironmentVariable;
        factory = new(connection, environmentName, database.GetConfiguration());
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

    public HttpContext SetHttpContextQuery(string rawQuery)
    {
        factory.ThrowIfNull();
        var context = new DefaultHttpContext();

        var parsed = QueryHelpers.ParseQuery(rawQuery);
        context.Request.Query = new QueryCollection(parsed);

        factory.ThrowIfNull();
        using var scope = factory!.Services.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        context.RequestServices = scope.ServiceProvider;
        accessor.HttpContext = context;

        return context;
    }

    public static Ulid GetUserId() => UserId;

    public static void RemoveUserId() => UserId = Ulid.Empty;
}
