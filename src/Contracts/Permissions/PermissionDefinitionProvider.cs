namespace Contracts.Permissions;

public abstract class PermissionDefinitionProvider
{
    public abstract void Define(PermissionDefinitionContext context);
}
