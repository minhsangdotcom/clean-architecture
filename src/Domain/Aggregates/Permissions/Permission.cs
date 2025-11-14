using Domain.Aggregates.Permissions.Enums;
using Domain.Aggregates.Permissions.Exceptions;
using Domain.Common;
using Mediator;

namespace Domain.Aggregates.Permissions;

public class Permission : AggregateRoot
{
    /// <summary>
    /// Unique code for this permission (e.g. "Product.Create", "Order.View").
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Human-readable name (e.g. "Create Product").
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? Group { get; private set; }

    public PermissionStatus Status { get; private set; } = PermissionStatus.Active;

    private Permission() { }

    private Permission(string code, string name, string? description = null, string? group = null)
    {
        Id = Ulid.NewUlid();
        Code = code;
        Name = name;
        Description = description;
        Group = group;
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

    protected override bool TryApplyDomainEvent(INotification domainEvent)
    {
        return false;
    }
}
