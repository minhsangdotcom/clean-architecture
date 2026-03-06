using ByteAether.Ulid;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Permissions.Specifications;

public class GetPermissionByIdSpecification : Specification<Permission>
{
    public GetPermissionByIdSpecification(List<Ulid> ids)
    {
        Query.Where(permission => ids.Contains(permission.Id));
    }
}
