using FluentValidation;

namespace Infrastructure.Services.Elasticsearch;

public class ElasticsearchSettingValidator : AbstractValidator<ElasticsearchSettings>
{
    public ElasticsearchSettingValidator()
    {
        RuleFor(x => x.Nodes)
            .Must(x => x.Count > 0)
            .WithMessage($"{nameof(ElasticsearchSettings.Nodes)} is not empty or null");
    }
}
