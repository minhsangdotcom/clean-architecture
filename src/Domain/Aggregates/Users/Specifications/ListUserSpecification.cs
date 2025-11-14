using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Users.Specifications;

public class ListUserSpecification : Specification<User>
{
    public ListUserSpecification()
    {
        string key = GetUniqueCachedKey();
        Query.EnableCache(key);
    }
}
