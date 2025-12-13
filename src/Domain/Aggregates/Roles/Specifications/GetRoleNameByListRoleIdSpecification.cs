using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Roles.Specifications;

public class GetRoleNameByListRoleIdSpecification : Specification<Role, string>
{
    public GetRoleNameByListRoleIdSpecification(List<Ulid> ids)
    {
        Query.Where(x => ids.Contains(x.Id)).Select(x => x.Name).AsNoTracking();
    }
}
