namespace Application.Contracts.Permissions;

public static class PermissionDefinitionContextExtension
{
    public static List<string> GetNestedPermissions(
        this PermissionDefinitionContext permissionDefinitionContext,
        IEnumerable<string> existentPermissions
    )
    {
        return
        [
            .. permissionDefinitionContext.Groups.SelectMany(x =>
                x.Value.Permissions.FindAll(p => existentPermissions.Contains(p.Code))
                    .Flatten()
                    .DistinctBy(p => p.Code)
                    .Select(p => p.Code)
            ),
        ];
    }
}
