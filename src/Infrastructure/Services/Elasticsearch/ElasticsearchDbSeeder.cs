using Elastic.Clients.Elasticsearch;
using FluentConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Elasticsearch;

public class ElasticsearchDbSeeder(
    ElasticsearchClient client,
    IndexTypeConfiguration configuration,
    ILogger<ElasticsearchDbSeeder> logger,
    IOptions<ElasticsearchSettings> options
)
{
    private readonly ElasticsearchSettings settings = options.Value;

    public async Task RunAsync()
    {
        // Apply elasticsearch configs and create index from defined entities
        await client.ElasticFluentConfigAsync(configuration.Configurations);
        await client.SeedingAsync(settings.PrefixIndex, logger);
    }
}
