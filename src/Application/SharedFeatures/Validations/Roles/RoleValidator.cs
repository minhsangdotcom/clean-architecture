using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Validators;
using Application.SharedFeatures.Requests.Roles;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using FluentValidation;

namespace Application.SharedFeatures.Validations.Roles;

public class RoleValidator(
    IEfUnitOfWork unitOfWork,
    IRequestContextProvider contextProvider,
    ITranslator<Messages> translator
) : FluentValidator<RoleUpsertCommand>(contextProvider, translator)
{
    protected sealed override void ApplyRules(
        IRequestContextProvider contextProvider,
        ITranslator<Messages> translator
    )
    {
        _ = Ulid.TryParse(contextProvider.GetId(), out Ulid id);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithTranslatedError(translator, RoleErrorMessages.RoleNameRequired)
            .MaximumLength(256)
            .WithTranslatedError(translator, RoleErrorMessages.RoleNameTooLong)
            // Create
            .MustAsync((name, ct) => IsNameAvailableAsync(name, cancellationToken: ct))
            .When(
                _ => contextProvider.GetHttpMethod() == HttpMethod.Post.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithTranslatedError(translator, RoleErrorMessages.RoleNameExistent)
            // Update
            .MustAsync((name, ct) => IsNameAvailableAsync(name, id, ct))
            .When(
                _ => contextProvider.GetHttpMethod() == HttpMethod.Put.ToString(),
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
            .ContainDistinctItems()
            .WithTranslatedError(translator, RoleErrorMessages.RolePermissionsUnique)
            .MustAsync((permissionIds, ct) => IsAllPermissionExistentAsync(permissionIds!, ct))
            .WithTranslatedError(translator, RoleErrorMessages.RolePermissionsExistent);
    }

    protected sealed override void ApplyRules(ITranslator<Messages> translator) { }

    private async Task<bool> IsNameAvailableAsync(
        string name,
        Ulid? excludeId = null,
        CancellationToken cancellationToken = default
    ) =>
        !await unitOfWork
            .Repository<Role>()
            .AnyAsync(
                x => x.Name == name && (!excludeId.HasValue || x.Id != excludeId),
                cancellationToken
            );

    private async Task<bool> IsAllPermissionExistentAsync(
        List<Ulid> permissionIds,
        CancellationToken cancellationToken = default
    ) =>
        await unitOfWork
            .Repository<Permission>()
            .CountAsync(x => permissionIds.Contains(x.Id), cancellationToken)
        == permissionIds.Count;
}
