using Domain.Aggregates.AuditLogs;
using FluentConfiguration.Configurations;
using Infrastructure.Services.Elasticsearch;

namespace Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IElasticsearchDocumentConfigure<AuditLog>
{
    public void Configure(
        ref ElasticsearchConfigBuilder<AuditLog> builder,
        string? prefix = null,
        string? delimiter = null
    )
    {
        builder.ToIndex(prefix: prefix);
        builder.HasKey(key => key.Id);

        builder.Properties(config =>
            config
                .Text(
                    t => t.Id,
                    config => config.Fields(f => f.Keyword(ElkPrefixProvider.KeywordPrefixName))
                )
                .Text(
                    txt => txt.Entity,
                    config => config.Fields(f => f.Keyword(ElkPrefixProvider.KeywordPrefixName))
                )
                .ByteNumber(b => b.Type)
                .Object(o => o.OldValue!)
                .Object(o => o.NewValue!)
                .Text(
                    txt => txt.ActionPerformBy!,
                    config =>
                        config.Fields(field => field.Keyword(ElkPrefixProvider.KeywordPrefixName))
                )
                .Keyword(d => d.CreatedAt)
                .Nested(
                    n => n.Agent!,
                    nest =>
                        nest.Properties(nestProp =>
                            nestProp
                                .Text(
                                    t => t.Id,
                                    config =>
                                        config.Fields(f =>
                                            f.Keyword(ElkPrefixProvider.KeywordPrefixName)
                                        )
                                )
                                .Text(
                                    t => t.Agent!.FirstName!,
                                    config =>
                                        config.Fields(f =>
                                            f.Keyword(ElkPrefixProvider.KeywordPrefixName)
                                        )
                                )
                                .Text(
                                    t => t.Agent!.LastName!,
                                    config =>
                                        config.Fields(f =>
                                            f.Keyword(ElkPrefixProvider.KeywordPrefixName)
                                        )
                                )
                                .Text(
                                    t => t.Agent!.Email!,
                                    config =>
                                        config.Fields(f =>
                                            f.Keyword(ElkPrefixProvider.KeywordPrefixName)
                                        )
                                )
                                .Date(d => d.Agent!.DayOfBirth!)
                                .ByteNumber(b => b.Agent!.Gender!)
                                .Keyword(x => x.Agent!.CreatedAt)
                        )
                )
        );
    }
}
