using System.Data.Common;
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

    public async Task InitializeAsync()
    {
        environmentName =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile(
                $"appsettings.Testing-{environmentName}.json",
                optional: true,
                reloadOnChange: true
            )
            .Build();

        connectionString = configuration["DatabaseSettings:DatabaseConnection"];
        connection = new NpgsqlConnection(connectionString);

        var options = new DbContextOptionsBuilder<TheDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        var context = new TheDbContext(options);
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

    public DbConnection GetConnection() => connection!;

    public string GetConnectionString() => connectionString!;

    public string GetEnvironmentVariable() => $"Testing-{environmentName}"!;

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
}
