using Microsoft.Extensions.Options;

namespace Infrastructure.Data.Settings;

public class ValidateDatabaseSetting : IValidateOptions<DatabaseSettings>
{
    public ValidateOptionsResult Validate(string? name, DatabaseSettings options)
    {
        if (string.IsNullOrWhiteSpace(options.DatabaseConnection))
        {
            return ValidateOptionsResult.Fail(
                $"{nameof(DatabaseSettings)} {nameof(options.DatabaseConnection)} must be not null or empty"
            );
        }

        return ValidateOptionsResult.Success;
    }
}
