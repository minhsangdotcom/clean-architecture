using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Users.Specifications;

public class GetUserByIdSpecification : Specification<User>
{
    public GetUserByIdSpecification(Ulid id)
    {
        Query
            .Where(x => x.Id == id)
            .Include(x => x.Roles)!
            .ThenInclude(x => x.Role)
            .ThenInclude(x => x!.Claims)
            .Include(x => x.Claims)
            .AsSplitQuery();
    }
}
