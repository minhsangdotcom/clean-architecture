using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Users.Commands.Create;
using Infrastructure.Data;
using Infrastructure.Data.Interceptors;
using Infrastructure.Services;
using Infrastructure.Services.Aws;
using Infrastructure.Services.Cache.DistributedCache;
using Infrastructure.Services.Cache.MemoryCache;
using Infrastructure.Services.Elasticsearch;
using Infrastructure.Services.Identity;
using Infrastructure.Services.Mail;
using Infrastructure.Services.Queue;
using Infrastructure.Services.Token;
using Infrastructure.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependencies(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDetection();
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.Configure<DatabaseSettings>(options =>
            configuration.GetSection(nameof(DatabaseSettings)).Bind(options)
        );
        services.TryAddSingleton<IValidateOptions<DatabaseSettings>, ValidateDatabaseSetting>();

        services.AddSingleton(sp =>
        {
            var databaseSettings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
            string connectionString = databaseSettings.DatabaseConnection!;
            return new NpgsqlDataSourceBuilder(connectionString).EnableDynamicJson().Build();
        });

        services
            .AddScoped<IDbContext, TheDbContext>()
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddSingleton<UpdateAuditableEntityInterceptor>()
            .AddSingleton<DispatchDomainEventInterceptor>();

        services.AddDbContextPool<TheDbContext>(
            (sp, options) =>
            {
                NpgsqlDataSource npgsqlDataSource = sp.GetRequiredService<NpgsqlDataSource>();
                options
                    .UseNpgsql(npgsqlDataSource)
                    .AddInterceptors(
                        sp.GetRequiredService<UpdateAuditableEntityInterceptor>(),
                        sp.GetRequiredService<DispatchDomainEventInterceptor>()
                    );
            }
        );

        // queue register
        services.AddQueue(configuration);

        services
            .AddAmazonS3(configuration)
            .AddHttpContextAccessor()
            .AddSingleton<ICurrentUser, CurrentUserService>()
            .AddScoped<IHttpContextAccessorService, HttpContextAccessorService>()
            .AddJwt(configuration)
            .AddElasticSearch(configuration)
            .AddIdentity()
            .AddMail(configuration)
            .AddMemoryCaching(configuration)
            .AddDistributedCache(configuration);

        return services;
    }
}
