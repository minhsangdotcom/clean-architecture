using FluentValidation;

namespace Infrastructure.Services.Aws;

public class AmazonS3SettingsValidator : AbstractValidator<AmazonS3Settings>
{
    public AmazonS3SettingsValidator()
    {
        RuleFor(x => x.ServiceUrl)
            .NotEmpty()
            .WithMessage($"{nameof(AmazonS3Settings.ServiceUrl)} must not be null or empty");

        RuleFor(x => x.AccessKey)
            .NotEmpty()
            .WithMessage($"{nameof(AmazonS3Settings.AccessKey)} must not be null or empty");

        RuleFor(x => x.SecretKey)
            .NotEmpty()
            .WithMessage($"{nameof(AmazonS3Settings.SecretKey)} must not be null or empty");

        RuleFor(x => x.Protocol)
            .NotEmpty()
            .WithMessage($"{nameof(AmazonS3Settings.Protocol)} is required")
            .IsInEnum()
            .WithMessage($"{nameof(AmazonS3Settings.Protocol)} is invalid");
    }
}
