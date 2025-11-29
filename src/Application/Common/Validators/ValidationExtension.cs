using System.Text.RegularExpressions;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Users;
using FluentValidation;

namespace Application.Common.Validators;

public static partial class ValidationExtension
{
    public static IRuleBuilderOptions<T, string?> UserEmailAvailable<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        IEfUnitOfWork unitOfWork,
        Ulid? excludeId = null
    )
    {
        return ruleBuilder.MustAsync(
            async (entity, email, context, ct) =>
            {
                bool exists = await unitOfWork
                    .Repository<User>()
                    .AnyAsync(
                        x => x.Email == email && (!excludeId.HasValue || x.Id != excludeId.Value),
                        ct
                    );

                return !exists;
            }
        );
    }

    public static bool IsValidUsername(this string username) =>
        UsernameValidationRegex().IsMatch(username);

    public static bool IsValidPassword(this string password) =>
        PasswordValidationRegex().IsMatch(password);

    public static bool IsValidPhoneNumber(this string phoneNumber) =>
        PhoneValidationRegex().IsMatch(phoneNumber);

    public static bool IsValidEmail(this string email) => EmailValidationRegex().IsMatch(email);

    [GeneratedRegex(@"^[a-zA-Z0-9_.]+$")]
    private static partial Regex UsernameValidationRegex();

    [GeneratedRegex(@"^((?=\S*?[A-Z])(?=\S*?[a-z])(?=\S*?[0-9]).{8,})\S$")]
    private static partial Regex PasswordValidationRegex();

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex EmailValidationRegex();

    [GeneratedRegex(@"^\+?\d{7,15}$")]
    private static partial Regex PhoneValidationRegex();
}
