namespace Application.Common.Interfaces.Seeder;

public interface IDbSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
