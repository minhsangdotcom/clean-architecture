using FluentValidation;

namespace Infrastructure.Data.Settings;

public class DatabaseSettingValidator : AbstractValidator<DatabaseSettings>
{
    public DatabaseSettingValidator()
    {
        RuleFor(x => x.DatabaseConnection)
            .NotEmpty()
            .WithMessage(
                $"{nameof(DatabaseSettings.DatabaseConnection)} must be not null or empty"
            );
    }
}
