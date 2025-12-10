using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Permissions.Specifications;

public class ListPermissionByIdSpecification : Specification<Permission>
{
    public ListPermissionByIdSpecification(List<Ulid> ids)
    {
        Query.Where(permission => ids.Contains(permission.Id)).AsNoTracking();
    }
}
