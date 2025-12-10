using System.Reflection;
using Application.Common.Interfaces.UnitOfWorks;
using DynamicQuery.Extensions;
using Infrastructure.Data.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Data;

public class TheDbContext(DbContextOptions<TheDbContext> options) : DbContext(options), IEfDbContext
{
    public DatabaseFacade DatabaseFacade => Database;

    public async Task<ITransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default
    )
    {
        IDbContextTransaction transaction = await Database.BeginTransactionAsync(cancellationToken);
        return new EfTransaction(transaction);
    }

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
