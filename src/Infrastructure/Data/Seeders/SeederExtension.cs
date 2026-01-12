using Application.Common.Interfaces.Seeder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data.Seeders;

public static class SeederExtension
{
    public static IServiceCollection AddSeeder(this IServiceCollection services)
    {
        return services.Scan(scan =>
            scan.FromAssemblyOf<DbSeederRunner>()
                .AddClasses(classes =>
                    classes.AssignableTo<IDbSeeder>().Where(type => !type.IsAbstract)
                )
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );
    }
}
