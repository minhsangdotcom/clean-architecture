using Application.Features.Common.Mapping.Roles;
using Domain.Aggregates.Roles;

namespace Application.Features.Common.Projections.Roles;

public class RoleDetailProjection : RoleProjection
{
    public ICollection<RoleClaimDetailProjection>? Claims { get; set; }

    public override void MappingFrom(Role role)
    {
        base.MappingFrom(role);
        //Claims = role.RoleClaims?.ToListRoleClaimDetailProjection();
    }
}
