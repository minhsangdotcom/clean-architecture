using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Messages;
using Application.SharedFeatures.Requests.Roles;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using DotNetCoreExtension.Extensions;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Application.SharedFeatures.Validators.Roles;

public class RoleValidator : AbstractValidator<RoleUpsertCommand>
{
    private readonly IEfUnitOfWork unitOfWork;
    private readonly IHttpContextAccessorService httpContextAccessorService;
    private readonly IStringLocalizer stringLocalizer;

    public RoleValidator(
        IEfUnitOfWork unitOfWork,
        IHttpContextAccessorService httpContextAccessorService,
        IStringLocalizer stringLocalizer
    )
    {
        this.unitOfWork = unitOfWork;
        this.httpContextAccessorService = httpContextAccessorService;
        this.stringLocalizer = stringLocalizer;
        ApplyRules();
    }

    private void ApplyRules()
    {
        _ = Ulid.TryParse(httpContextAccessorService.GetId(), out Ulid id);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                RoleErrorMessages.RoleNameRequired,
                stringLocalizer[RoleErrorMessages.RoleNameRequired]
            ))
            .MaximumLength(256)
            .WithState(_ => new ErrorReason(
                RoleErrorMessages.RoleNameTooLong,
                stringLocalizer[RoleErrorMessages.RoleNameTooLong]
            ))
            .MustAsync((name, ct) => IsNameAvailableAsync(name, cancellationToken: ct))
            .When(
                _ => httpContextAccessorService.GetHttpMethod() == HttpMethod.Post.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithState(_ => new ErrorReason(
                RoleErrorMessages.RoleNameExistentOnCreate,
                stringLocalizer[RoleErrorMessages.RoleNameExistentOnCreate]
            ))
            .MustAsync((name, ct) => IsNameAvailableAsync(name, id, ct))
            .When(
                _ => httpContextAccessorService.GetHttpMethod() == HttpMethod.Put.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithState(_ => new ErrorReason(
                RoleErrorMessages.RoleNameExistentOnUpdate,
                stringLocalizer[RoleErrorMessages.RoleNameExistentOnUpdate]
            ));

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description), ApplyConditionTo.CurrentValidator)
            .WithState(_ => new ErrorReason(
                RoleErrorMessages.RoleDescriptionTooLong,
                stringLocalizer[RoleErrorMessages.RoleDescriptionTooLong]
            ));

        RuleFor(x => x.PermissionIds)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                RoleErrorMessages.RolePermissionsRequired,
                stringLocalizer[RoleErrorMessages.RolePermissionsRequired]
            ))
            .Must(x => x!.Distinct().Count() == x!.Count)
            .WithState(_ => new ErrorReason(
                RoleErrorMessages.RolePermissionsUnique,
                stringLocalizer[RoleErrorMessages.RolePermissionsUnique]
            ))
            .MustAsync((permissionIds, ct) => IsAllPermissionExistentAsync(permissionIds!, ct))
            .WithState(_ => new ErrorReason(
                RoleErrorMessages.RolePermissionsExistent,
                stringLocalizer[RoleErrorMessages.RolePermissionsExistent]
            ));
    }

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
