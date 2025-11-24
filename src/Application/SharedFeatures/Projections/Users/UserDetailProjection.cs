using Application.SharedFeatures.Mapping.Permissions;
using Application.SharedFeatures.Mapping.Roles;
using Application.SharedFeatures.Projections.Permissions;
using Application.SharedFeatures.Projections.Roles;
using Domain.Aggregates.Users;

namespace Application.SharedFeatures.Projections.Users;

public class UserDetailProjection : UserProjection
{
    public IReadOnlyList<RoleDetailProjection>? Roles { get; set; }
    public IReadOnlyList<PermissionDetailProjection>? Permissions { get; set; }

    public override void MappingFrom(User user)
    {
        base.MappingFrom(user);
        Roles = user.Roles.Select(userRole => userRole.Role!).ToListRoleDetailProjection();
        Permissions = user
            .Permissions.Select(p => p.Permission!)
            ?.ToListPermissionDetailProjection();
    }
}
