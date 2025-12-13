using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TheDbContext>
{
    public TheDbContext CreateDbContext(string[] args)
    {
        string environment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(
                @Directory.GetCurrentDirectory() + $"/../Api/appsettings.{environment}.json"
            )
            .Build();
        string? connectionString = configuration.GetValue<string>(
            "DatabaseSettings:DatabaseConnection"
        );
        DbContextOptionsBuilder<TheDbContext> builder = new();
        builder.UseNpgsql(connectionString);
        return new TheDbContext(builder.Options);
    }
}
