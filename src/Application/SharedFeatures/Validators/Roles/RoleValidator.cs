using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Localization;
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
    private readonly IMessageTranslatorService translator;

    public RoleValidator(
        IEfUnitOfWork unitOfWork,
        IHttpContextAccessorService httpContextAccessorService,
        IMessageTranslatorService translator
    )
    {
        this.unitOfWork = unitOfWork;
        this.httpContextAccessorService = httpContextAccessorService;
        this.translator = translator;
        ApplyRules();
    }

    private void ApplyRules()
    {
        _ = Ulid.TryParse(httpContextAccessorService.GetId(), out Ulid id);

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
                _ => httpContextAccessorService.GetHttpMethod() == HttpMethod.Post.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithState(_ => new ErrorReason(
                RoleErrorMessages.RoleNameExistent,
                translator.Translate(RoleErrorMessages.RoleNameExistent)
            ))
            .MustAsync((name, ct) => IsNameAvailableAsync(name, id, ct))
            .When(
                _ => httpContextAccessorService.GetHttpMethod() == HttpMethod.Put.ToString(),
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
