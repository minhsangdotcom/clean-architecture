namespace Contracts.Permissions;

public class SystemPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(PermissionDefinitionContext context)
    {
        PermissionGroupDefinition roleGroup = context.AddGroup("RoleManagement", "Role Management");

        PermissionDefinition listRole = roleGroup.AddPermission(
            PermissionNames.Permission.Generate(
                PermissionNames.PermissionResource.Role,
                PermissionNames.PermissionAction.List
            ),
            "View list role"
        );

        PermissionDefinition detailRole = roleGroup.AddPermission(
            PermissionNames.Permission.Generate(
                PermissionNames.PermissionResource.Role,
                PermissionNames.PermissionAction.Detail
            ),
            "View role details"
        );
        detailRole.AddChild(listRole);

        PermissionDefinition createRole = roleGroup.AddPermission(
            PermissionNames.Permission.Generate(
                PermissionNames.PermissionResource.Role,
                PermissionNames.PermissionAction.Create
            ),
            "Create Role"
        );
        createRole.AddChild(listRole);

        PermissionDefinition updateRole = roleGroup.AddPermission(
            PermissionNames.Permission.Generate(
                PermissionNames.PermissionResource.Role,
                PermissionNames.PermissionAction.Update
            ),
            "Update Role"
        );
        updateRole.AddChild(detailRole);

        PermissionDefinition deleteRole = roleGroup.AddPermission(
            PermissionNames.Permission.Generate(
                PermissionNames.PermissionResource.Role,
                PermissionNames.PermissionAction.Delete
            ),
            "Delete Role"
        );
        deleteRole.AddChild(listRole);

        PermissionDefinition testTole = roleGroup.AddPermission(
            PermissionNames.Permission.Generate(
                PermissionNames.PermissionResource.Role,
                PermissionNames.PermissionAction.Test
            ),
            "Test Role"
        );
    }
}
