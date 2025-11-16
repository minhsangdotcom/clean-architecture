using Application.Common.Interfaces.Services.Identity;
using Contracts.ApiWrapper;
using Domain.Aggregates.Roles;
using Mediator;

namespace Application.Features.Roles.Queries.List;

public class ListRoleHandler(IRoleManager manager)
    : IRequestHandler<ListRoleQuery, Result<IEnumerable<ListRoleResponse>>>
{
    public async ValueTask<Result<IEnumerable<ListRoleResponse>>> Handle(
        ListRoleQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<Role> roles = await manager.GetAllAsync(cancellationToken);
        return Result<IEnumerable<ListRoleResponse>>.Success(roles.ToListRoleResponse());
    }
}
