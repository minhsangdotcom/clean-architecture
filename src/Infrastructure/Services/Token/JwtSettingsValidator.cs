using FluentValidation;

namespace Infrastructure.Services.Token;

public class JwtSettingsValidator : AbstractValidator<JwtSettings>
{
    public JwtSettingsValidator()
    {
        RuleFor(x => x.Default)
            .NotEmpty()
            .WithMessage($"{nameof(JwtSettings)}.{nameof(JwtSettings.Default)} is required")
            .SetValidator(new JwtTypeValidator()!);
    }
}

public class JwtTypeValidator : AbstractValidator<JwtType>
{
    public JwtTypeValidator()
    {
        RuleFor(x => x.SecretKey)
            .NotEmpty()
            .WithMessage($"{nameof(JwtType)}.{nameof(JwtType.SecretKey)} is required");
    }
}
