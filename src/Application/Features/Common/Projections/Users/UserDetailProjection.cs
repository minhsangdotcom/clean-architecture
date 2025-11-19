using Application.Features.Common.Mapping.Permissions;
using Application.Features.Common.Mapping.Roles;
using Application.Features.Common.Projections.Permissions;
using Application.Features.Common.Projections.Roles;
using Domain.Aggregates.Users;

namespace Application.Features.Common.Projections.Users;

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
