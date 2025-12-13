using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Users.Specifications;

public class GetUserByIdentifierSpecification : Specification<User>
{
    public GetUserByIdentifierSpecification(string identifier)
    {
        Query.Where(x => x.Username == identifier || x.Email == identifier).AsNoTracking();
    }
}
