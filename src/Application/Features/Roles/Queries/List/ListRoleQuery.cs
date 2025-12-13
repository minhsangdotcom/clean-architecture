using Application.Contracts.ApiWrapper;
using Application.Contracts.Binds;
using Application.Contracts.Dtos.Requests;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Roles.Queries.List;

public class ListRoleQuery()
    : QueryParamRequest,
        IQueryBinding<ListRoleQuery>,
        IRequest<Result<IReadOnlyList<ListRoleResponse>>>
{
    public static ValueTask<ListRoleQuery> BindAsync(HttpContext context) =>
        ValueTask.FromResult(QueryParamRequestExtension.Bind<ListRoleQuery>(context));
}
