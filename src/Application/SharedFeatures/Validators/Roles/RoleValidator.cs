using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Validators;
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
            .WithTranslatedError(translator, RoleErrorMessages.RoleNameRequired)
            .MaximumLength(256)
            .WithTranslatedError(translator, RoleErrorMessages.RoleNameTooLong)
            // Create
            .MustAsync((name, ct) => IsNameAvailableAsync(name, cancellationToken: ct))
            .When(
                _ => httpContextAccessor.GetHttpMethod() == HttpMethod.Post.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithTranslatedError(translator, RoleErrorMessages.RoleNameExistent)
            // Update
            .MustAsync((name, ct) => IsNameAvailableAsync(name, id, ct))
            .When(
                _ => httpContextAccessor.GetHttpMethod() == HttpMethod.Put.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithTranslatedError(translator, RoleErrorMessages.RoleNameExistent);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description), ApplyConditionTo.CurrentValidator)
            .WithTranslatedError(translator, RoleErrorMessages.RoleDescriptionTooLong);

        RuleFor(x => x.PermissionIds)
            .NotEmpty()
            .WithTranslatedError(translator, RoleErrorMessages.RolePermissionsRequired)
            .Must(x => x!.Distinct().Count() == x!.Count)
            .WithTranslatedError(translator, RoleErrorMessages.RolePermissionsUnique)
            .MustAsync((permissionIds, ct) => IsAllPermissionExistentAsync(permissionIds!, ct))
            .WithTranslatedError(translator, RoleErrorMessages.RolePermissionsExistent);
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
