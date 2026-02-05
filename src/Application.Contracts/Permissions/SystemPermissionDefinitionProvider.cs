namespace Application.Contracts.Permissions;

public class SystemPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(PermissionDefinitionContext context)
    {
        #region Role permission
        PermissionGroupDefinition roleGroup = context.AddGroup("RoleManagement", "Role Management");

        PermissionDefinition listRole = roleGroup.AddPermission(
            PermissionNames.PermissionGenerator.Generate(
                PermissionNames.PermissionResource.Role,
                PermissionNames.PermissionAction.List
            ),
            "View list role"
        );

        PermissionDefinition detailRole = roleGroup.AddPermission(
            PermissionNames.PermissionGenerator.Generate(
                PermissionNames.PermissionResource.Role,
                PermissionNames.PermissionAction.Detail
            ),
            "View role details"
        );
        detailRole.AddChild(listRole);

        PermissionDefinition createRole = roleGroup.AddPermission(
            PermissionNames.PermissionGenerator.Generate(
                PermissionNames.PermissionResource.Role,
                PermissionNames.PermissionAction.Create
            ),
            "Create Role"
        );
        createRole.AddChild(listRole);

        PermissionDefinition updateRole = roleGroup.AddPermission(
            PermissionNames.PermissionGenerator.Generate(
                PermissionNames.PermissionResource.Role,
                PermissionNames.PermissionAction.Update
            ),
            "Update Role"
        );
        updateRole.AddChild(detailRole);

        PermissionDefinition deleteRole = roleGroup.AddPermission(
            PermissionNames.PermissionGenerator.Generate(
                PermissionNames.PermissionResource.Role,
                PermissionNames.PermissionAction.Delete
            ),
            "Delete Role"
        );
        deleteRole.AddChild(listRole);
        #endregion

        #region User permission
        PermissionGroupDefinition userGroup = context.AddGroup("UserManagement", "User Management");

        PermissionDefinition listUser = userGroup.AddPermission(
            PermissionNames.PermissionGenerator.Generate(
                PermissionNames.PermissionResource.User,
                PermissionNames.PermissionAction.List
            ),
            "View list User"
        );

        PermissionDefinition detailUser = userGroup.AddPermission(
            PermissionNames.PermissionGenerator.Generate(
                PermissionNames.PermissionResource.User,
                PermissionNames.PermissionAction.Detail
            ),
            "View User details"
        );
        detailUser.AddChild(listUser);

        PermissionDefinition createUser = userGroup.AddPermission(
            PermissionNames.PermissionGenerator.Generate(
                PermissionNames.PermissionResource.User,
                PermissionNames.PermissionAction.Create
            ),
            "Create User"
        );
        createUser.AddChild(listUser);

        PermissionDefinition updateUser = userGroup.AddPermission(
            PermissionNames.PermissionGenerator.Generate(
                PermissionNames.PermissionResource.User,
                PermissionNames.PermissionAction.Update
            ),
            "Update User"
        );
        updateUser.AddChild(detailUser);

        PermissionDefinition deleteUser = userGroup.AddPermission(
            PermissionNames.PermissionGenerator.Generate(
                PermissionNames.PermissionResource.User,
                PermissionNames.PermissionAction.Delete
            ),
            "Delete User"
        );
        deleteUser.AddChild(listUser);
        #endregion

        PermissionGroupDefinition queueGroup = context.AddGroup(
            "QueueManagement",
            "Queue Management"
        );
        queueGroup.AddPermission(
            PermissionNames.PermissionGenerator.Generate(
                PermissionNames.PermissionResource.QueueLog,
                PermissionNames.PermissionAction.List
            ),
            "List Queue"
        );
    }
}
