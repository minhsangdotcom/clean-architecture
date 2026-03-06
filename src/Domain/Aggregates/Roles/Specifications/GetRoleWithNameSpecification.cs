using ByteAether.Ulid;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Roles.Specifications;

public class GetRoleWithNameSpecification : Specification<Role>
{
    public GetRoleWithNameSpecification(string name, Ulid? excludeId = null)
    {
        Query.Where(x => x.Name == name && (!excludeId.HasValue || x.Id != excludeId));
    }
}
