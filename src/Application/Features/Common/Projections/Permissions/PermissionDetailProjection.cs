using Domain.Aggregates.Permissions;

namespace Application.Features.Common.Projections.Permissions;

public class PermissionDetailProjection : PermissionProjection
{
    public override void MappingFrom(Permission permission)
    {
        base.MappingFrom(permission);
    }
}
