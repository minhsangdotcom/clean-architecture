using System.Text.RegularExpressions;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Domain.Aggregates.Users;
using FluentValidation;

namespace Application.Common.Validators;

public static partial class ValidationExtension
{
    public static IRuleBuilderOptions<T, string?> BeUniqueUserEmail<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        IEfUnitOfWork unitOfWork,
        Ulid? excludeId = null
    ) =>
        ruleBuilder.MustAsync(
            (entity, email, context, ct) =>
            {
                return unitOfWork
                    .Repository<User>()
                    .AnyAsync(
                        x => x.Email == email && (!excludeId.HasValue || x.Id != excludeId.Value),
                        ct
                    )
                    .ContinueWith(t => t.Result == false, ct);
            }
        );

    public static IRuleBuilderOptions<T, string?> BeValidUsername<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    ) =>
        ruleBuilder.Must(username =>
            string.IsNullOrWhiteSpace(username) || UsernameValidationRegex().IsMatch(username)
        );

    public static IRuleBuilderOptions<T, string?> BeValidPassword<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    ) =>
        ruleBuilder.Must(pwd =>
            string.IsNullOrWhiteSpace(pwd) || PasswordValidationRegex().IsMatch(pwd)
        );

    public static IRuleBuilderOptions<T, string?> BeValidPhoneNumber<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    ) =>
        ruleBuilder.Must(phoneNumber =>
            string.IsNullOrWhiteSpace(phoneNumber) || PhoneValidationRegex().IsMatch(phoneNumber)
        );

    public static IRuleBuilderOptions<T, string?> BeValidEmail<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    ) =>
        ruleBuilder.Must(email =>
            string.IsNullOrWhiteSpace(email) || EmailValidationRegex().IsMatch(email)
        );

    public static IRuleBuilderOptions<T, IEnumerable<TItem>?> ContainDistinctItems<T, TItem>(
        this IRuleBuilder<T, IEnumerable<TItem>?> ruleBuilder
    ) =>
        ruleBuilder.Must(list =>
            list == null || !list.Any() || list!.Distinct().Count() == list.Count()
        );

    public static IRuleBuilderOptions<T, List<TItem>?> ContainDistinctItems<T, TItem>(
        this IRuleBuilder<T, List<TItem>?> ruleBuilder
    ) =>
        ruleBuilder.Must(list =>
            list == null || list.Count == 0 || list.Distinct().Count() == list.Count
        );

    public static IRuleBuilderOptions<T, TProperty> WithTranslatedError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        IMessageTranslatorService translator,
        string key
    ) => rule.WithState(_ => new ErrorReason(key, translator.Translate(key)));

    [GeneratedRegex(@"^[a-zA-Z0-9_.]+$")]
    private static partial Regex UsernameValidationRegex();

    [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9])\S{8,}$")]
    private static partial Regex PasswordValidationRegex();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailValidationRegex();

    [GeneratedRegex(@"^\+?\d{7,15}$")]
    private static partial Regex PhoneValidationRegex();
}
