using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Validators;
using Application.SharedFeatures.Requests.Users;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using FluentValidation;

namespace Application.SharedFeatures.Validators.Users;

public class UserValidator(
    IEfUnitOfWork unitOfWork,
    IHttpContextAccessorService httpContextAccessorService,
    IMessageTranslatorService translator
) : FluentValidator<UserUpsertCommand>(httpContextAccessorService, translator)
{
    protected sealed override void ApplyRules(
        IHttpContextAccessorService contextAccessor,
        IMessageTranslatorService translator
    )
    {
        _ = Ulid.TryParse(contextAccessor.GetId(), out Ulid id);

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
            .BeUniqueUserEmail(unitOfWork)
            .When(
                _ => contextAccessor.GetHttpMethod() == HttpMethod.Post.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithTranslatedError(translator, UserErrorMessages.UserEmailExistent)
            // PUT
            .BeUniqueUserEmail(unitOfWork, id)
            .When(
                _ => contextAccessor.GetHttpMethod() == HttpMethod.Put.ToString(),
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

    protected override void ApplyRules(IMessageTranslatorService translator) { }

    private async Task<bool> IsRolesAvailableAsync(
        List<Ulid> roles,
        CancellationToken cancellationToken = default
    ) =>
        await unitOfWork.Repository<Role>().CountAsync(x => roles.Contains(x.Id), cancellationToken)
        == roles.Count;

    private async Task<bool> IsPermissionsAvailableAsync(
        List<Ulid> permissions,
        CancellationToken cancellationToken
    ) =>
        await unitOfWork
            .Repository<Permission>()
            .CountAsync(x => permissions.Contains(x.Id), cancellationToken) == permissions.Count;
}
