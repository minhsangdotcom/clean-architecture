using Application.Features.Common.Mapping.Permissions;
using Application.Features.Common.Projections.Permissions;
using Domain.Aggregates.Roles;

namespace Application.Features.Common.Projections.Roles;

public class RoleDetailProjection : RoleProjection
{
    public IReadOnlyList<PermissionDetailProjection>? Permissions { get; set; }

    public override void MappingFrom(Role role)
    {
        base.MappingFrom(role);
        Permissions = role
            .Permissions.Select(x => x.Permission!)
            ?.ToListPermissionDetailProjection();
    }
}
