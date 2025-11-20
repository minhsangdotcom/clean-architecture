using Ardalis.GuardClauses;
using Domain.Aggregates.Users;
using Domain.Common;
using Mediator;

namespace Domain.Aggregates.Roles;

public class Role : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<UserRole>? Users { get; private set; } = [];
    public ICollection<RoleClaim>? Claims { get; private set; } = [];
    public ICollection<RolePermission> Permissions { get; private set; } = [];

    private Role() { }

    public Role(string name, string? description = null)
    {
        Name = Guard.Against.NullOrEmpty(name, nameof(name));
        Description = description;
    }

    // for seeding purpose
    public Role(
        Ulid id,
        string name,
        List<RolePermission> permissions,
        string? description,
        string createdBy
    )
    {
        Id = id;
        Name = Guard.Against.NullOrEmpty(name, nameof(name));
        Description = description;
        Permissions = [.. Permissions.Concat(permissions)];
        CreatedBy = createdBy;
    }

    public void Update(string name, string? description)
    {
        SetName(name);
        Description = description;
    }

    public void SetName(string name) => Name = Guard.Against.NullOrEmpty(name, nameof(name));

    public void ClearAllPermissions()
    {
        Permissions.Clear();
    }

    protected override bool TryApplyDomainEvent(INotification domainEvent)
    {
        return true;
    }
}
