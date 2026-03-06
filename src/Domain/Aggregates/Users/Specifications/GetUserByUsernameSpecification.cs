using ByteAether.Ulid;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Users.Specifications;

public class GetUserByUsernameSpecification : Specification<User>
{
    public GetUserByUsernameSpecification(string username, Ulid? excludeId = null)
    {
        Query.Where(x => x.Username == username && (!excludeId.HasValue || x.Id != excludeId));
    }
}
