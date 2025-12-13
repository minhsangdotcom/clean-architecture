namespace Application.Contracts.Permissions;

public class PermissionDefinitionContext
{
    private readonly Dictionary<string, PermissionGroupDefinition> _groups = [];

    public IReadOnlyDictionary<string, PermissionGroupDefinition> Groups => _groups;

    public PermissionGroupDefinition AddGroup(string name, string displayName)
    {
        var group = new PermissionGroupDefinition(name, displayName);
        _groups[name] = group;
        return group;
    }
}
