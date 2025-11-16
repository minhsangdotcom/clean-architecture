using Application.Features.Common.Projections.Permissions;
using Domain.Aggregates.Permissions;

namespace Application.Features.Common.Mapping.Permissions;

public static class PermissionMapping
{
    public static IReadOnlyCollection<PermissionDetailProjection> ToPermissionDetailProjection(
        this IEnumerable<Permission> permissions
    ) =>
        [
            .. permissions.Select(x =>
            {
                PermissionDetailProjection projection = new();
                projection.MappingFrom(x);
                return projection;
            }),
        ];
}
