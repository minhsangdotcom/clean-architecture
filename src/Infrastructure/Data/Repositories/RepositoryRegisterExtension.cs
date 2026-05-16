using Application.Common.Interfaces.Repositories;
using Infrastructure.Data.Repositories.EfCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Repositories;

namespace Infrastructure.Data.Repositories;

public static class RepositoryRegisterExtension
{
    public static IServiceCollection AddSpecificRepositories(this IServiceCollection services)
    {
        services.Scan(scan =>
            scan.FromAssemblyOf<TheDbContext>()
                .AddClasses(c =>
                    c.Where(t =>
                        !t.IsAbstract
                        && t.GetInterfaces()
                            .Any(i =>
                                typeof(IRepository).IsAssignableFrom(i) && i != typeof(IRepository)
                            )
                    )
                )
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.AddScoped<IRepositoryFactory, RepositoryFactory>();

        return services;
    }
}
