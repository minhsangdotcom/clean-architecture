using Application.SharedFeatures.Projections.Roles;
using Domain.Aggregates.Roles;

namespace Application.SharedFeatures.Mapping.Roles;

public static class RoleMapping
{
    public static IReadOnlyList<RoleDetailProjection>? ToListRoleDetailProjection(
        this IEnumerable<Role> roles
    ) =>
        [
            .. roles.Select(role =>
            {
                RoleDetailProjection projection = new();
                projection.MappingFrom(role);
                return projection;
            }),
        ];
}
