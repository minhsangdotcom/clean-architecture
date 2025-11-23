using Domain.Aggregates.Permissions.Enums;
using Domain.Aggregates.Permissions.Exceptions;
using Mediator;
using SharedKernel.DomainEvents;
using SharedKernel.Entities;

namespace Domain.Aggregates.Permissions;

public class Permission : AggregateRoot
{
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? Group { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? EffectiveFrom { get; private set; }

    public DateTimeOffset? EffectiveTo { get; private set; }

    public PermissionStatus Status { get; private set; } = PermissionStatus.Active;

    private Permission() { }

    public Permission(
        string code,
        string name,
        string? description = null,
        string? group = null,
        DateTimeOffset? effectiveFrom = null,
        DateTimeOffset? effectiveTo = null,
        string? createdBy = null
    )
    {
        Code = code;
        Name = name;
        Description = description;
        Group = group;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;

        if (!string.IsNullOrWhiteSpace(createdBy))
        {
            CreatedBy = createdBy!;
        }
    }

    public void Deactivate()
    {
        if (Status != PermissionStatus.Active)
        {
            throw new PermissionAlreadyInactiveException(Code);
        }
        Status = PermissionStatus.Inactive;
    }

    public void Activate()
    {
        if (Status != PermissionStatus.Inactive)
        {
            throw new PermissionAlreadyActiveException(Code);
        }
        Status = PermissionStatus.Inactive;
    }

    protected override bool TryApplyDomainEvent(IDomainEvent domainEvent)
    {
        throw new NotImplementedException();
    }
}
