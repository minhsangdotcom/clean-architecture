using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Validators;
using Application.SharedFeatures.Requests.Users;
using ByteAether.Ulid;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Permissions.Specifications;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Roles.Specifications;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using FluentValidation;

namespace Application.SharedFeatures.Validations.Users;

public class UserValidator(
    IEfUnitOfWork unitOfWork,
    IRequestContextProvider contextProvider,
    ITranslator<Messages> translator
) : FluentValidator<UserUpsertCommand>(contextProvider, translator)
{
    protected sealed override void ApplyRules(
        IRequestContextProvider contextProvider,
        ITranslator<Messages> translator
    )
    {
        _ = Ulid.TryParse(contextProvider.GetId(), null, out Ulid id);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserLastNameRequired)
            .MaximumLength(256)
            .WithTranslatedError(translator, UserErrorMessages.UserLastNameTooLong);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserFirstNameRequired)
            .MaximumLength(256)
            .WithTranslatedError(translator, UserErrorMessages.UserFirstNameTooLong);

        RuleFor(x => x.PhoneNumber)
            .BeValidPhoneNumber()
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithTranslatedError(translator, UserErrorMessages.UserPhoneNumberInvalid);

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserEmailRequired)
            .BeValidEmail()
            .WithTranslatedError(translator, UserErrorMessages.UserEmailInvalid)
            // POST
            .MustAsync((x, cancellationToken) => IsAvailableEmailAsync(x!, null, cancellationToken))
            .When(
                _ => contextProvider.GetHttpMethod() == HttpMethod.Post.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithTranslatedError(translator, UserErrorMessages.UserEmailExistent)
            // PUT
            .MustAsync((x, cancellationToken) => IsAvailableEmailAsync(x!, id, cancellationToken))
            .When(
                _ => contextProvider.GetHttpMethod() == HttpMethod.Put.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithTranslatedError(translator, UserErrorMessages.UserEmailExistent);

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserStatusRequired)
            .IsInEnum()
            .WithTranslatedError(translator, UserErrorMessages.UserStatusNotInEnum);

        RuleFor(x => x.Roles)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserRolesRequired)
            .ContainDistinctItems()
            .WithTranslatedError(translator, UserErrorMessages.UserRolesNotUnique)
            .MustAsync((roles, ct) => IsRolesAvailableAsync(roles!, ct))
            .WithTranslatedError(translator, UserErrorMessages.UserRolesNotFound);

        When(
            x => x.Permissions != null,
            () =>
            {
                RuleFor(r => r.Permissions)
                    .Cascade(CascadeMode.Stop)
                    .ContainDistinctItems()
                    .WithTranslatedError(translator, UserErrorMessages.UserPermissionsNotUnique)
                    .MustAsync((p, ct) => IsPermissionsAvailableAsync(p!, ct))
                    .WithTranslatedError(translator, UserErrorMessages.UserPermissionsNotFound);
            }
        );
    }

    protected override void ApplyRules(ITranslator<Messages> translator) { }

    private async Task<bool> IsRolesAvailableAsync(
        List<Ulid> roles,
        CancellationToken cancellationToken = default
    ) =>
        await unitOfWork
            .ReadRepository<Role>()
            .CountAsync(new GetRoleByIdSpecification(roles), cancellationToken) == roles.Count;

    private async Task<bool> IsPermissionsAvailableAsync(
        List<Ulid> permissions,
        CancellationToken cancellationToken
    ) =>
        await unitOfWork
            .ReadRepository<Permission>()
            .CountAsync(new GetPermissionByIdSpecification(permissions), cancellationToken)
        == permissions.Count;

    private async Task<bool> IsAvailableEmailAsync(
        string email,
        Ulid? excludeId = null,
        CancellationToken cancellationToken = default
    )
    {
        return !(
            await unitOfWork
                .ReadRepository<User>()
                .AnyAsync(new GetUserByEmailSpecification(email, excludeId), cancellationToken)
        );
    }
}
