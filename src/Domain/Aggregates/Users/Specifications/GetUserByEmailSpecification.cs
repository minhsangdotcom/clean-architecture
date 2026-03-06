using ByteAether.Ulid;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Users.Specifications;

public class GetUserByEmailSpecification : Specification<User>
{
    public GetUserByEmailSpecification(string email, Ulid? excludeId = null)
    {
        Query.Where(x => x.Email == email && (!excludeId.HasValue || x.Id != excludeId.Value));
    }
}
