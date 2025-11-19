using Application.Features.Common.Projections.Roles;
using Application.Features.Common.Requests.Roles;
using Domain.Aggregates.Roles;

namespace Application.Features.Common.Mapping.Roles;

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
