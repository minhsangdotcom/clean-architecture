using Ardalis.GuardClauses;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles.Exceptions;
using Domain.Aggregates.Users;
using Domain.Common;
using DotNetCoreExtension.Extensions;
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

    public Role(string name, string? description)
    {
        Name = Guard.Against.NullOrEmpty(name, nameof(name)).ToScreamingSnakeCase();
        Description = description;
    }

    public void Update(string name, string? description)
    {
        SetName(name);
        Description = description;
    }

    public void SetName(string name)
    {
        Guard.Against.NullOrEmpty(name, nameof(name));
        Name = name.ToScreamingSnakeCase();
    }

    public void GrantPermission(Permission permission)
    {
        if (Permissions.Any(p => p.PermissionId == permission.Id))
        {
            throw new PermissionAlreadyGrantedException(Id, permission.Id);
        }

        Permissions.Add(new RolePermission { RoleId = Id, PermissionId = permission.Id });
    }

    public void RevokePermission(Permission permission)
    {
        var existentPermission =
            Permissions.FirstOrDefault(p => p.PermissionId == permission.Id)
            ?? throw new PermissionNotGrantedException(Id, permission.Id);
        Permissions.Remove(existentPermission);
    }

    public void ClearAllPermissions()
    {
        Permissions.Clear();
    }

    protected override bool TryApplyDomainEvent(INotification domainEvent)
    {
        return true;
    }
}
