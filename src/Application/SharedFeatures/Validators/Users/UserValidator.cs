using Application.Common.ErrorCodes;
using Application.Common.Extensions;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Messages;
using Application.SharedFeatures.Requests.Users;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Application.SharedFeatures.Validators.Users;

public partial class UserValidator : AbstractValidator<UserUpsertCommand>
{
    private readonly IEfUnitOfWork unitOfWork;
    private readonly IStringLocalizer stringLocalizer;

    public UserValidator(IEfUnitOfWork unitOfWork, IStringLocalizer stringLocalizer)
    {
        this.unitOfWork = unitOfWork;
        this.stringLocalizer = stringLocalizer;
        ApplyRules();
    }

    private void ApplyRules()
    {
        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserLastNameRequired,
                stringLocalizer[UserErrorMessages.UserLastNameRequired]
            ))
            .MaximumLength(256)
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserLastNameTooLong,
                stringLocalizer[UserErrorMessages.UserLastNameTooLong]
            ));

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserFirstNameRequired,
                stringLocalizer[UserErrorMessages.UserFirstNameRequired]
            ))
            .MaximumLength(256)
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserFirstNameTooLong,
                stringLocalizer[UserErrorMessages.UserFirstNameTooLong]
            ));

        RuleFor(x => x.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .Must(x => x!.IsValidPhoneNumber())
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserPhoneNumberInvalid,
                stringLocalizer[UserErrorMessages.UserPhoneNumberInvalid]
            ));

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserStatusRequired,
                stringLocalizer[UserErrorMessages.UserStatusRequired]
            ))
            .IsInEnum()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserStatusNotInEnum,
                stringLocalizer[UserErrorMessages.UserStatusNotInEnum]
            ));

        RuleFor(x => x.Roles)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserRolesRequired,
                stringLocalizer[UserErrorMessages.UserRolesRequired]
            ))
            .Must(x => x!.Distinct().Count() == x!.Count)
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserRolesNotUnique,
                stringLocalizer[UserErrorMessages.UserRolesNotUnique]
            ))
            .MustAsync((roles, ct) => IsRolesAvailableAsync(roles!, ct))
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserRolesNotFound,
                stringLocalizer[UserErrorMessages.UserRolesNotFound]
            ));

        When(
            x => x.Permissions != null,
            () =>
            {
                RuleFor(r => r.Permissions)
                    .Must((p, _) => p.Permissions!.Distinct().Count() == p.Permissions!.Count)
                    .WithState(_ => new ErrorReason(
                        UserErrorMessages.UserPermissionsNotUnique,
                        stringLocalizer[UserErrorMessages.UserPermissionsNotUnique]
                    ))
                    .MustAsync((p, ct) => IsPermissionsAvailableAsync(p!, ct))
                    .WithState(_ => new ErrorReason(
                        UserErrorMessages.UserPermissionsNotFound,
                        stringLocalizer[UserErrorMessages.UserPermissionsNotFound]
                    ));
            }
        );
    }

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
