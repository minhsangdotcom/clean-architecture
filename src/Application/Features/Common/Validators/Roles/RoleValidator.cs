using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Requests.Roles;
using CaseConverter;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using DotNetCoreExtension.Extensions;
using FluentValidation;
using SharedKernel.Common.Messages;

namespace Application.Features.Common.Validators.Roles;

public class RoleValidator : AbstractValidator<RoleUpsertCommand>
{
    private readonly IEfUnitOfWork unitOfWork;
    private readonly IHttpContextAccessorService httpContextAccessorService;

    public RoleValidator(
        IEfUnitOfWork unitOfWork,
        IHttpContextAccessorService httpContextAccessorService
    )
    {
        this.unitOfWork = unitOfWork;
        this.httpContextAccessorService = httpContextAccessorService;
        ApplyRules();
    }

    private void ApplyRules()
    {
        _ = Ulid.TryParse(httpContextAccessorService.GetId(), out Ulid id);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithState(x =>
                Messenger
                    .Create<RoleUpsertCommand>(nameof(Role))
                    .Property(x => x.Name!)
                    .Negative()
                    .Message(MessageType.Null)
                    .Build()
            )
            .MaximumLength(256)
            .WithState(x =>
                Messenger
                    .Create<RoleUpsertCommand>(nameof(Role))
                    .Property(x => x.Name!)
                    .Message(MessageType.MaximumLength)
                    .Build()
            )
            .MustAsync(
                (name, cancellationToken) =>
                    IsNameAvailableAsync(name, cancellationToken: cancellationToken)
            )
            .When(
                _ => httpContextAccessorService.GetHttpMethod() == HttpMethod.Post.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithState(x =>
                Messenger
                    .Create<RoleUpsertCommand>(nameof(Role))
                    .Property(x => x.Name!)
                    .Message(MessageType.Existence)
                    .Build()
            )
            .MustAsync(
                (name, cancellationToken) => IsNameAvailableAsync(name, id, cancellationToken)
            )
            .When(
                _ => httpContextAccessorService.GetHttpMethod() == HttpMethod.Put.ToString(),
                ApplyConditionTo.CurrentValidator
            )
            .WithState(x =>
                Messenger
                    .Create<RoleUpsertCommand>(nameof(Role))
                    .Property(x => x.Name!)
                    .Message(MessageType.Existence)
                    .Build()
            );

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description != null, ApplyConditionTo.CurrentValidator)
            .WithState(x =>
                Messenger
                    .Create<RoleUpsertCommand>(nameof(Role))
                    .Property(x => x.Description!)
                    .Message(MessageType.MaximumLength)
                    .Build()
            );

        RuleFor(x => x.PermissionIds)
            .NotEmpty()
            .WithState(x =>
                Messenger
                    .Create<Role>()
                    .Property(x => x.Permissions)
                    .Negative()
                    .Message(MessageType.Null)
                    .Build()
            )
            .Must(x => x!.Distinct().Count() == x!.Count)
            .WithState(x =>
                Messenger
                    .Create<Role>()
                    .Property(x => x.Permissions)
                    .Message(MessageType.Unique)
                    .Negative()
                    .Build()
            )
            .MustAsync(
                (permissionIds, cancellationToken) =>
                    IsAnyPermissionExistentAsync(permissionIds!, cancellationToken)
            )
            .WithState(x =>
                Messenger
                    .Create<Role>()
                    .Property(x => x.Permissions)
                    .Message(MessageType.Existence)
                    .Negative()
                    .Build()
            );
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
                x => (!id.HasValue && x.Name == caseName) || (x.Id != id && x.Name == caseName),
                cancellationToken
            );
    }

    private async Task<bool> IsAnyPermissionExistentAsync(
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
