namespace Contracts.Permissions;

public class PermissionGroupDefinition(string name, string displayName)
{
    public string Name { get; } = name;
    public string DisplayName { get; } = displayName;

    public List<PermissionDefinition> Permissions { get; } = [];

    public PermissionDefinition AddPermission(string code, string name, string? description = null)
    {
        var permission = new PermissionDefinition(code, name, description);
        Permissions.Add(permission);
        return permission;
    }
}
