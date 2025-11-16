namespace Contracts.Permissions;

public static class PermissionDefinitionExtensions
{
    public static IEnumerable<PermissionDefinition> Flatten(
        this IEnumerable<PermissionDefinition> permissions
    )
    {
        foreach (var permission in permissions)
        {
            yield return permission;

            if (permission.Children.Count > 0)
            {
                foreach (var child in permission.Children.Flatten())
                {
                    yield return child;
                }
            }
        }
    }
}
