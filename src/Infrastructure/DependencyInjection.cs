using System.Reflection;
using FluentValidation;
using Infrastructure.common.validator;
using Infrastructure.Constants;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Data.Seeders;
using Infrastructure.Services.Aws;
using Infrastructure.Services.Cache.DistributedCache;
using Infrastructure.Services.Cache.MemoryCache;
using Infrastructure.Services.Elasticsearch;
using Infrastructure.Services.Identity;
using Infrastructure.Services.Mail;
using Infrastructure.Services.Queue;
using Infrastructure.Services.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        // for IOption validation
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Stop;
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddOptionsWithFluentValidation<DatabaseSettings>(
            configuration.GetSection(nameof(DatabaseSettings))
        );

        CurrentProvider? provider = configuration
            .GetSection($"{nameof(DatabaseSettings)}:{nameof(DatabaseSettings.Provider)}")
            .Get<CurrentProvider>();

        if (
            provider != null
            && DatabaseConfiguration.relationalProviders.Contains(provider.ToString()!)
        )
        {
            // EFCore register
            services.AddEfCoreRelationalDatabase(provider.Value);
        }
        else
        {
            // non-relational db register
        }

        // queue register
        services.AddQueue(configuration);

        services
            .AddAmazonS3(configuration)
            .AddJwt(configuration)
            .AddElasticSearch(configuration)
            .AddIdentity(configuration)
            .AddMail(configuration)
            .AddMemoryCaching(configuration)
            .AddDistributedCache(configuration)
            .AddSpecificRepositories()
            .Configure<HostOptions>(options =>
            {
                options.ServicesStartConcurrently = true;
                options.ServicesStopConcurrently = true;
            })
            .AddSeeder();

        return services;
    }
}
