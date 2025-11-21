using Application.Common.Interfaces.Services.Identity;
using Application.Contracts.ApiWrapper;
using Domain.Aggregates.Roles;
using Mediator;

namespace Application.Features.Roles.Queries.List;

public class ListRoleHandler(IRoleManager manager)
    : IRequestHandler<ListRoleQuery, Result<IReadOnlyList<ListRoleResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<ListRoleResponse>>> Handle(
        ListRoleQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<Role> roles = await manager.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<ListRoleResponse>>.Success(roles.ToListRoleResponse());
    }
}
