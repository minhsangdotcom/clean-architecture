using System.Data.Common;
using Ardalis.GuardClauses;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;

namespace Application.SubcutaneousTests;

public class PostgreSqlDatabase : IDatabase
{
    private NpgsqlConnection? connection;

    private string? connectionString;
    private Respawner? respawner;

    private string? environmentName;

    public DbConnection Connection => connection!;

    public string ConnectionString => connectionString!;

    public string EnvironmentVariable => GetEnvironment(environmentName!);

    public IConfiguration GetConfiguration()
    {
        string path = Directory.GetCurrentDirectory();
        return new ConfigurationBuilder()
            .SetBasePath(path)
            .AddJsonFile(
                $"appsettings.{GetEnvironment(environmentName!)}.json",
                optional: false,
                reloadOnChange: true
            )
            .Build();
    }

    public async Task InitializeAsync()
    {
        environmentName =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        IConfiguration configuration = GetConfiguration();
        connectionString = configuration[
            "DatabaseSettings:Relational:PostgreSQL:DatabaseConnection"
        ];
        Guard.Against.Null(connectionString);

        connection = new NpgsqlConnection(connectionString);
        var options = new DbContextOptionsBuilder<TheDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        TheDbContext context = new(options);
        context.Database.EnsureDeleted();
        context.Database.Migrate();

        await connection.OpenAsync();
        respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                // don't remove these tables
                TablesToIgnore = ["__EFMigrationsHistory"],
                SchemasToInclude = ["public"],
            }
        );
        await connection.CloseAsync();
    }

    public async Task ResetAsync()
    {
        if (respawner != null && connection != null)
        {
            await connection.OpenAsync();
            await respawner.ResetAsync(connection);
            await connection.CloseAsync();
        }
    }

    public async Task DisposeAsync()
    {
        if (connection != null)
        {
            await connection.CloseAsync();
            await connection.DisposeAsync();
        }
    }

    private static string GetEnvironment(string env)
    {
        if (env == "Development")
        {
            return "Test";
        }
        return env;
    }
}
