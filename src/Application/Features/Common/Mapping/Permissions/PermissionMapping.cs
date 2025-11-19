using Application.Features.Common.Projections.Permissions;
using Domain.Aggregates.Permissions;

namespace Application.Features.Common.Mapping.Permissions;

public static class PermissionMapping
{
    public static IReadOnlyList<PermissionDetailProjection> ToListPermissionDetailProjection(
        this IEnumerable<Permission> permissions
    ) =>
        [
            .. permissions.Select(permission =>
            {
                PermissionDetailProjection projection = new();
                projection.MappingFrom(permission);
                return projection;
            }),
        ];
}
