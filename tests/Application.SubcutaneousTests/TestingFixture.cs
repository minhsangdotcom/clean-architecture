using Application.SubcutaneousTests.Extensions;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.SubcutaneousTests;

public partial class TestingFixture : IAsyncLifetime
{
    private CustomWebApplicationFactory<Program> factory = null!;
    private PostgreSqlDatabase database = null!;

    private string BASE_URL = string.Empty;

    private HttpClient client = null!;
    private static Ulid UserId;

    public async Task InitializeAsync()
    {
        string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        string environmentName = GetEnvironment(env);
        IConfiguration configuration = GetConfiguration(environmentName);
        database = new(configuration);
        await database.InitializeAsync();

        string connectionString = database.ConnectionString;
        factory = new(connectionString, environmentName, configuration);
        BASE_URL = configuration["urls"] ?? "http://localhost:8080/api/v1";
        client = factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        await database.DisposeAsync();
        if (factory != null)
        {
            await factory.DisposeAsync();
        }
        client?.Dispose();
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
        using var scope = factory.Services.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(request);
    }

    public HttpContext SetHttpContextQuery(string rawQuery)
    {
        factory.ThrowIfNull();
        var context = new DefaultHttpContext();

        var parsed = QueryHelpers.ParseQuery(rawQuery);
        context.Request.Query = new QueryCollection(parsed);

        using var scope = factory.Services.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        context.RequestServices = scope.ServiceProvider;
        accessor.HttpContext = context;

        return context;
    }

    public static Ulid GetUserId() => UserId;

    public static void RemoveUserId() => UserId = Ulid.Empty;

    private static IConfiguration GetConfiguration(string envName)
    {
        string path = AppContext.BaseDirectory;
        return new ConfigurationBuilder()
            .SetBasePath(path)
            .AddJsonFile($"appsettings.{envName}.json", optional: false, reloadOnChange: true)
            .Build();
    }

    private static string GetEnvironment(string env) => env == "Development" ? "Test" : env;
}
