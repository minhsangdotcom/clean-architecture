using Application.Common.Interfaces.UnitOfWorks;
using Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Data;

public static class DatabaseExtension
{
    public static IServiceCollection AddEfCoreRelationalDatabase(
        this IServiceCollection services,
        CurrentProvider provider
    )
    {
        if (provider == CurrentProvider.PostgreSQL)
        {
            services.AddSingleton(sp =>
            {
                var databaseSettings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
                RelationalProviderSetting providerSetting = databaseSettings
                    .Relational!
                    .PostgreSQL!;
                return new NpgsqlDataSourceBuilder(providerSetting.ConnectionString)
                    .EnableDynamicJson()
                    .Build();
            });
        }

        services
            .AddScoped<IEfDbContext, TheDbContext>()
            .AddScoped<IEfUnitOfWork, EfUnitOfWork>()
            .AddSingleton<UpdateAuditableEntityInterceptor>()
            .AddSingleton<DispatchDomainEventInterceptor>();

        services.AddDbContextPool<TheDbContext>(
            (sp, options) =>
            {
                var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
                switch (settings.Provider)
                {
                    case CurrentProvider.PostgreSQL:
                    {
                        NpgsqlDataSource npgsqlDataSource =
                            sp.GetRequiredService<NpgsqlDataSource>();
                        options.UseNpgsql(npgsqlDataSource);
                        break;
                    }
                    default:
                        throw new NotSupportedException(
                            $"Database provider {settings.Provider} is not supported."
                        );
                }

                options.AddInterceptors(
                    sp.GetRequiredService<UpdateAuditableEntityInterceptor>(),
                    sp.GetRequiredService<DispatchDomainEventInterceptor>()
                );
            }
        );

        return services;
    }
}
