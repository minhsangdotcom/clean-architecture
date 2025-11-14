using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Users.Specifications;

public class GetUserByIdIncludePasswordResetRequestSpecification : Specification<User>
{
    public GetUserByIdIncludePasswordResetRequestSpecification(Ulid id)
    {
        Query.Where(x => x.Id == id).Include(x => x.PasswordResetRequests).AsNoTracking();
    }
}
