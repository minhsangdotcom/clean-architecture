namespace Contracts.Permissions;

public class PermissionDefinition(string code, string name, string? description = null)
{
    public string Code { get; } = code;
    public string Name { get; } = name;
    public string? Description { get; } = description;

    public List<PermissionDefinition> Children { get; } = [];

    public PermissionDefinition AddChild(string code, string name, string? description = null)
    {
        PermissionDefinition child = new(name, code, description);
        Children.Add(child);
        return child;
    }

    public PermissionDefinition AddChild(PermissionDefinition child)
    {
        Children.Add(child);
        return child;
    }
}
