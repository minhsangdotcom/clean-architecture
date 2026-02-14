using Ardalis.GuardClauses;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;

namespace Application.SubcutaneousTests;

public class PostgreSqlDatabase(IConfiguration configuration) : IDatabase
{
    private NpgsqlConnection connection = null!;
    private Respawner respawner = null!;
    private string connectionString = null!;

    public string ConnectionString => connectionString;

    public async Task InitializeAsync()
    {
        connectionString =
            configuration["DatabaseSettings:Relational:PostgreSQL:ConnectionString"] ?? null!;
        Guard.Against.Null(connectionString);

        connection = new NpgsqlConnection(connectionString);
        var options = new DbContextOptionsBuilder<TheDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        TheDbContext context = new(options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();

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
}
