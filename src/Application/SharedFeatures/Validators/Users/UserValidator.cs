using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Validators;
using Application.Contracts.ApiWrapper;
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
    protected override void ApplyRules(
        IHttpContextAccessorService contextAccessor,
        IMessageTranslatorService translator
    )
    {
        _ = Ulid.TryParse(contextAccessor.GetId(), out Ulid id);
        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserLastNameRequired,
                translator.Translate(UserErrorMessages.UserLastNameRequired)
            ))
            .MaximumLength(256)
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserLastNameTooLong,
                translator.Translate(UserErrorMessages.UserLastNameTooLong)
            ));

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserFirstNameRequired,
                translator.Translate(UserErrorMessages.UserFirstNameRequired)
            ))
            .MaximumLength(256)
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserFirstNameTooLong,
                translator.Translate(UserErrorMessages.UserFirstNameTooLong)
            ));

        RuleFor(x => x.PhoneNumber)
            .Must(x => x!.IsValidPhoneNumber())
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserPhoneNumberInvalid,
                translator.Translate(UserErrorMessages.UserPhoneNumberInvalid)
            ));

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserEmailRequired,
                translator.Translate(UserErrorMessages.UserEmailRequired)
            ))
            .Must(x => x!.IsValidEmail())
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserEmailInvalid,
                translator.Translate(UserErrorMessages.UserEmailInvalid)
            ))
            .UserEmailAvailable(unitOfWork)
            .When(
                _ => contextAccessor.GetHttpMethod() == HttpMethod.Post.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserEmailExistent,
                translator.Translate(UserErrorMessages.UserEmailExistent)
            ))
            .UserEmailAvailable(unitOfWork, id)
            .When(
                _ => contextAccessor.GetHttpMethod() == HttpMethod.Put.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserEmailExistent,
                translator.Translate(UserErrorMessages.UserEmailExistent)
            ));

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserStatusRequired,
                translator.Translate(UserErrorMessages.UserStatusRequired)
            ))
            .IsInEnum()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserStatusNotInEnum,
                translator.Translate(UserErrorMessages.UserStatusNotInEnum)
            ));

        RuleFor(x => x.Roles)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserRolesRequired,
                translator.Translate(UserErrorMessages.UserRolesRequired)
            ))
            .Must(x => x!.Distinct().Count() == x!.Count)
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserRolesNotUnique,
                translator.Translate(UserErrorMessages.UserRolesNotUnique)
            ))
            .MustAsync((roles, ct) => IsRolesAvailableAsync(roles!, ct))
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserRolesNotFound,
                translator.Translate(UserErrorMessages.UserRolesNotFound)
            ));

        When(
            x => x.Permissions != null,
            () =>
            {
                RuleFor(r => r.Permissions)
                    .Cascade(CascadeMode.Stop)
                    .Must((p, _) => p.Permissions!.Distinct().Count() == p.Permissions!.Count)
                    .WithState(_ => new ErrorReason(
                        UserErrorMessages.UserPermissionsNotUnique,
                        translator.Translate(UserErrorMessages.UserPermissionsNotUnique)
                    ))
                    .MustAsync((p, ct) => IsPermissionsAvailableAsync(p!, ct))
                    .WithState(_ => new ErrorReason(
                        UserErrorMessages.UserPermissionsNotFound,
                        translator.Translate(UserErrorMessages.UserPermissionsNotFound)
                    ));
            }
        );
    }

    protected override void ApplyRules(IMessageTranslatorService translator) { }

    private async Task<bool> IsRolesAvailableAsync(
        IEnumerable<Ulid> roles,
        CancellationToken cancellationToken = default
    ) =>
        await unitOfWork.Repository<Role>().CountAsync(x => roles.Contains(x.Id), cancellationToken)
        == roles.Count();

    private async Task<bool> IsPermissionsAvailableAsync(
        IEnumerable<Ulid> permissions,
        CancellationToken cancellationToken
    ) =>
        await unitOfWork
            .Repository<Permission>()
            .CountAsync(x => permissions.Contains(x.Id), cancellationToken) == permissions.Count();
}
