using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Messages;
using Application.Features.Common.Requests.Roles;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using DotNetCoreExtension.Extensions;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Application.Features.Common.Validators.Roles;

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
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<Role>()
                    .Property(x => x.Name)
                    .Negative()
                    .WithError(MessageErrorType.Required)
                    .GetFullMessage();
                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            })
            .MaximumLength(256)
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<Role>(nameof(Role))
                    .Property(x => x.Name!)
                    .WithError(MessageErrorType.TooLong)
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            })
            .MustAsync(
                (name, cancellationToken) =>
                    IsNameAvailableAsync(name, cancellationToken: cancellationToken)
            )
            .When(
                _ => httpContextAccessorService.GetHttpMethod() == HttpMethod.Post.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<Role>()
                    .Property(x => x.Name!)
                    .WithError(MessageErrorType.Existent)
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            })
            .MustAsync(
                (name, cancellationToken) => IsNameAvailableAsync(name, id, cancellationToken)
            )
            .When(
                _ => httpContextAccessorService.GetHttpMethod() == HttpMethod.Put.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<Role>()
                    .Property(x => x.Name!)
                    .WithError(MessageErrorType.Existent)
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            });

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description), ApplyConditionTo.CurrentValidator)
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<Role>()
                    .Property(x => x.Description!)
                    .WithError(MessageErrorType.TooLong)
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            });

        RuleFor(x => x.PermissionIds)
            .NotEmpty()
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<Role>()
                    .Property(x => x.Permissions)
                    .Negative()
                    .WithError(MessageErrorType.Required)
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            })
            .Must(x => x!.Distinct().Count() == x!.Count)
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<Role>()
                    .Property(x => x.Permissions)
                    .Negative()
                    .WithError(MessageErrorType.Unique)
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            })
            .MustAsync(
                (permissionIds, cancellationToken) =>
                    IsAllPermissionExistentAsync(permissionIds!, cancellationToken)
            )
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<Role>()
                    .Property(x => x.Permissions)
                    .Negative()
                    .WithError(MessageErrorType.Existent)
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            });
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
                x => (!id.HasValue && x.Name == caseName) || x.Name == caseName,
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
