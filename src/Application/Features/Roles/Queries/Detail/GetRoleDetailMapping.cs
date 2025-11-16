using Domain.Aggregates.Roles;

namespace Application.Features.Roles.Queries.Detail;

public static class GetRoleDetailMapping
{
    public static RoleDetailResponse ToRoleDetailResponse(this Role role)
    {
        RoleDetailResponse detailResponse = new();
        detailResponse.MappingFrom(role);

        return detailResponse;
    }
}
