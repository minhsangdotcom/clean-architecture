using Elastic.Clients.Elasticsearch;
using FluentConfiguration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Elasticsearch;

public class ElasticDataSeeder(
    ElasticsearchClient client,
    ElasticConfiguration configuration,
    IOptions<ElasticsearchSettings> options
) : IHostedLifecycleService
{
    private readonly ElasticsearchSettings settings = options.Value;

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await client.ElasticFluentConfigAsync(configuration.Configurations);
        await client.SeedingAsync(settings.PrefixIndex);
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
