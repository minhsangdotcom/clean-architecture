using Application.Common.Interfaces.Seeder;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Seeders;

public class DbSeederRunner(IEnumerable<IDbSeeder> seeders, ILogger<DbSeederRunner> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        foreach (IDbSeeder seeder in seeders)
        {
            logger.LogInformation("Running seeder: {Seeder}", seeder.GetType().Name);
            await seeder.SeedAsync(cancellationToken);
        }
    }
}
