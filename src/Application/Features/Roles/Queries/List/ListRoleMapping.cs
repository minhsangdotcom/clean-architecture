using Domain.Aggregates.Roles;

namespace Application.Features.Roles.Queries.List;

public static class ListRoleMapping
{
    public static IReadOnlyList<ListRoleResponse> ToListRoleResponse(this IEnumerable<Role> roles)
    {
        return
        [
            .. roles.Select(role =>
            {
                ListRoleResponse listRole = new();
                listRole.MappingFrom(role);
                return listRole;
            }),
        ];
    }
}
