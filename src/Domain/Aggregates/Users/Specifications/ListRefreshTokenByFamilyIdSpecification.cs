using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Users.Specifications;

public class ListRefreshTokenByFamilyIdSpecification : Specification<UserRefreshToken>
{
    public ListRefreshTokenByFamilyIdSpecification(string familyId, Ulid userId)
    {
        Query.Where(x => x.FamilyId == familyId && x.UserId == userId).Include(x => x.User);
    }
}
