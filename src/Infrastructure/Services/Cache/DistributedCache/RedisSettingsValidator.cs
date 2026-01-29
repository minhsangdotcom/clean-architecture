using FluentValidation;

namespace Infrastructure.Services.Cache.DistributedCache;

public class RedisSettingsValidator : AbstractValidator<RedisSettings>
{
    public RedisSettingsValidator()
    {
        RuleFor(x => x.Host)
            .NotEmpty()
            .WithMessage($"{nameof(RedisSettings.Host)} is not empty or null");
    }
}
