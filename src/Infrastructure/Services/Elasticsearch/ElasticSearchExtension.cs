using System.Reflection;
using Application.Common.Interfaces.Services.Elasticsearch;
using CaseConverter;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using FluentConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.Elasticsearch;

public static class ElasticSearchExtension
{
    public static IServiceCollection AddElasticSearch(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ElasticsearchSettings elasticsearch =
            configuration.GetSection(nameof(ElasticsearchSettings)).Get<ElasticsearchSettings>()
            ?? new();

        if (elasticsearch.IsEnabled)
        {
            List<Uri> nodes = elasticsearch.Nodes.ConvertAll(x => new Uri(x));
            StaticNodePool pool = new(nodes);
            string? userName = elasticsearch.Username;
            string? password = elasticsearch.Password;

            ElasticsearchClientSettings settings = new(pool);
            if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
            {
                settings
                    .Authentication(new BasicAuthentication(userName, password))
                    // without ssl trust
                    .ServerCertificateValidationCallback((o, certificate, chain, errors) => true)
                    .ServerCertificateValidationCallback(CertificateValidations.AllowAll);
            }

            //get all configs of entity
            List<ElasticConfigureResult> elkConfigBuilder =
            [
                .. ElasticsearchRegisterHelper.GetElasticsearchConfigBuilder(
                    Assembly.GetExecutingAssembly(),
                    elasticsearch.PrefixIndex.ToKebabCase()
                ),
            ];
            // add configurations of id, ignore properties
            ElasticsearchRegisterHelper.ConfigureConnectionSettings(ref settings, elkConfigBuilder);

            ElasticsearchClient client = new(settings);

            client.ElasticFluentConfigAsync(elkConfigBuilder).GetAwaiter();
            client.SeedingAsync(elasticsearch.PrefixIndex).GetAwaiter();

            services
                .AddSingleton(client)
                .AddSingleton<IElasticsearchServiceFactory, ElasticsearchServiceFactory>();
        }

        return services;
    }
}
