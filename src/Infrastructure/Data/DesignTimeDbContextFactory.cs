using System.Reflection;
using FluentValidation;
using Infrastructure.common.validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TheDbContext>
{
    public TheDbContext CreateDbContext(string[] args)
    {
        string environment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Api"))
            .AddJsonFile($"appsettings.{environment}.json")
            .AddEnvironmentVariables()
            .Build();

        ServiceCollection services = new();
        services.AddScoped<IValidator<DatabaseSettings>, DatabaseSettingValidator>();
        services
            .AddOptions<DatabaseSettings>()
            .Bind(configuration.GetSection(nameof(DatabaseSettings)))
            .ValidateFluentValidation()
            .ValidateOnStart();
        ServiceProvider provider = services.BuildServiceProvider();

        var databaseSettingOptions = provider.GetRequiredService<IOptions<DatabaseSettings>>();
        DatabaseSettings databaseSettings = databaseSettingOptions.Value;

        DbContextOptionsBuilder<TheDbContext> builder = new();
        switch (databaseSettings.Provider)
        {
            case CurrentProvider.PostgreSQL:
                builder.UseNpgsql(databaseSettings.Relational!.PostgreSQL!.ConnectionString);
                break;
            default:
                throw new NotSupportedException();
        }

        return new TheDbContext(builder.Options);
    }
}
