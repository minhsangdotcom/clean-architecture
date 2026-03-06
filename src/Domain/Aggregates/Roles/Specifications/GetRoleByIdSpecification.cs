using ByteAether.Ulid;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Roles.Specifications;

public class GetRoleByIdSpecification : Specification<Role>
{
    public GetRoleByIdSpecification(IEnumerable<Ulid> ids)
    {
        Query.Where(x => ids.Contains(x.Id));
    }
}
