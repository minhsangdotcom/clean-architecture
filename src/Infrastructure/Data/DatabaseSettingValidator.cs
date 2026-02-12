using FluentValidation;
using Infrastructure.Constants;

namespace Infrastructure.Data;

public class DatabaseSettingValidator : AbstractValidator<DatabaseSettings>
{
    public DatabaseSettingValidator()
    {
        List<string> providers = [.. Enum.GetValues<CurrentProvider>().Select(x => x.ToString())];

        RuleFor(x => x.Provider)
            .NotEmpty()
            .WithMessage($"{nameof(DatabaseSettings.Provider)} is required")
            .IsInEnum()
            .WithMessage(
                $"{nameof(DatabaseSettings.Provider)} must be ${string.Join(",", providers)}"
            );

        RuleFor(x => x.Relational)
            .NotEmpty()
            .When(
                x => DatabaseConfiguration.relationalProviders.Contains(x.Provider.ToString()),
                ApplyConditionTo.CurrentValidator
            )
            .WithMessage($"Relational DB is required");

        RuleFor(x => x.NonRelational)
            .NotEmpty()
            .When(
                x => !DatabaseConfiguration.relationalProviders.Contains(x.Provider.ToString()),
                ApplyConditionTo.CurrentValidator
            )
            .WithMessage($"Non-Relational DB is required");

        When(
            x => x.Provider == CurrentProvider.PostgreSQL,
            () =>
            {
                RuleFor(x => x.Relational!.PostgreSQL)
                    .NotNull()
                    .WithMessage("PostgreSQL configuration is required.")
                    .SetValidator(new RelationalProviderSettingValidator()!);
            }
        );

        When(
            x => x.Provider == CurrentProvider.MongoDB,
            () =>
            {
                RuleFor(x => x.NonRelational!.MongoDB)
                    .NotNull()
                    .WithMessage("MongoDB configuration is required.")
                    .SetValidator(new NonRelationalProviderSettingValidator()!);
            }
        );
    }
}

public class RelationalProviderSettingValidator : AbstractValidator<RelationalProviderSetting>
{
    public RelationalProviderSettingValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithMessage($"{nameof(RelationalProviderSetting.ConnectionString)} is required");
    }
}

public class NonRelationalProviderSettingValidator : AbstractValidator<NonRelationalProviderSetting>
{
    public NonRelationalProviderSettingValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithMessage($"{nameof(NonRelationalProviderSetting.ConnectionString)} is required");
    }
}
