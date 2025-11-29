using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Validators;
using Application.Contracts.ApiWrapper;
using Application.SharedFeatures.Requests.Roles;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using DotNetCoreExtension.Extensions;
using FluentValidation;

namespace Application.SharedFeatures.Validators.Roles;

public class RoleValidator(
    IEfUnitOfWork unitOfWork,
    IHttpContextAccessorService httpContextAccessor,
    IMessageTranslatorService translator
) : FluentValidator<RoleUpsertCommand>(httpContextAccessor, translator)
{
    protected sealed override void ApplyRules(
        IHttpContextAccessorService httpContextAccessor,
        IMessageTranslatorService translator
    )
    {
        _ = Ulid.TryParse(httpContextAccessor.GetId(), out Ulid id);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                RoleErrorMessages.RoleNameRequired,
                translator.Translate(RoleErrorMessages.RoleNameRequired)
            ))
            .MaximumLength(256)
            .WithState(_ => new ErrorReason(
                RoleErrorMessages.RoleNameTooLong,
                translator.Translate(RoleErrorMessages.RoleNameTooLong)
            ))
            .MustAsync((name, ct) => IsNameAvailableAsync(name, cancellationToken: ct))
            .When(
                _ => httpContextAccessor.GetHttpMethod() == HttpMethod.Post.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithState(_ => new ErrorReason(
                RoleErrorMessages.RoleNameExistent,
                translator.Translate(RoleErrorMessages.RoleNameExistent)
            ))
            .MustAsync((name, ct) => IsNameAvailableAsync(name, id, ct))
            .When(
                _ => httpContextAccessor.GetHttpMethod() == HttpMethod.Put.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithState(_ => new ErrorReason(
                RoleErrorMessages.RoleNameExistent,
                translator.Translate(RoleErrorMessages.RoleNameExistent)
            ));

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description), ApplyConditionTo.CurrentValidator)
            .WithState(_ => new ErrorReason(
                RoleErrorMessages.RoleDescriptionTooLong,
                translator.Translate(RoleErrorMessages.RoleDescriptionTooLong)
            ));

        RuleFor(x => x.PermissionIds)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                RoleErrorMessages.RolePermissionsRequired,
                translator.Translate(RoleErrorMessages.RolePermissionsRequired)
            ))
            .Must(x => x!.Distinct().Count() == x!.Count)
            .WithState(_ => new ErrorReason(
                RoleErrorMessages.RolePermissionsUnique,
                translator.Translate(RoleErrorMessages.RolePermissionsUnique)
            ))
            .MustAsync((permissionIds, ct) => IsAllPermissionExistentAsync(permissionIds!, ct))
            .WithState(_ => new ErrorReason(
                RoleErrorMessages.RolePermissionsExistent,
                translator.Translate(RoleErrorMessages.RolePermissionsExistent)
            ));
    }

    protected sealed override void ApplyRules(IMessageTranslatorService translator) { }

    private async Task<bool> IsNameAvailableAsync(
        string name,
        Ulid? id = null,
        CancellationToken cancellationToken = default
    )
    {
        string caseName = name.ToScreamingSnakeCase();
        return !await unitOfWork
            .Repository<Role>()
            .AnyAsync(
                x => !id.HasValue && x.Name == caseName || x.Name == caseName,
                cancellationToken
            );
    }

    private async Task<bool> IsAllPermissionExistentAsync(
        List<Ulid> permissionIds,
        CancellationToken cancellationToken = default
    )
    {
        return await unitOfWork
                .Repository<Permission>()
                .CountAsync(x => permissionIds.Contains(x.Id), cancellationToken)
            == permissionIds.Count;
    }
}
