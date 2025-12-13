using Application.SharedFeatures.Mapping.Permissions;
using Application.SharedFeatures.Projections.Permissions;
using Domain.Aggregates.Roles;

namespace Application.SharedFeatures.Projections.Roles;

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
