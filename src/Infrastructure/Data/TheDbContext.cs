using System.Reflection;
using Application.Common.Interfaces.Repositories;
using Domain.Common;
using DynamicQuery.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Infrastructure.Data;

public class TheDbContext(DbContextOptions<TheDbContext> options) : DbContext(options), IDbContext
{
    public DatabaseFacade DatabaseFacade => Database;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.HasPostgresExtension("citext");

        modelBuilder.HasDbFunction(
            typeof(PostgresDbFunctionExtensions).GetMethod(
                nameof(PostgresDbFunctionExtensions.Unaccent)
            )!
        );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSnakeCaseNamingConvention();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.Properties<Ulid>().HaveConversion<UlidToStringConverter>();
}
