using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.common.validator;

public class FluentValidateOptions<TOptions>(
    IServiceProvider serviceProvider,
    string? validationName
) : IValidateOptions<TOptions>
    where TOptions : class
{
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        if (validationName is not null && validationName != name)
        {
            return ValidateOptionsResult.Skip;
        }
        ArgumentNullException.ThrowIfNull(options);

        using var scope = serviceProvider.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IValidator<TOptions>>();

        var result = validator.Validate(options);
        if (result.IsValid)
        {
            return ValidateOptionsResult.Success;
        }

        var errors = new List<string>();
        foreach (var failure in result.Errors)
        {
            errors.Add(
                $"Validation failed for {options.GetType().Name}.{failure.PropertyName} "
                    + $"with the error: {failure.ErrorMessage}"
            );
        }

        return ValidateOptionsResult.Fail(errors);
    }
}
