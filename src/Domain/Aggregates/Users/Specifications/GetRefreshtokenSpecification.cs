using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Users.Specifications;

public class GetRefreshtokenSpecification : Specification<UserRefreshToken>
{
    public GetRefreshtokenSpecification(string token, Ulid userId)
    {
        Query.Where(x => x.UserId == userId && x.RefreshToken == token).Include(x => x.User);
    }
}
