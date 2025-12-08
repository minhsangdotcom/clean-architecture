using System.Reflection;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Binds;
using Application.Contracts.Dtos.Requests;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Permissions;

public class ListPermissionQuery
    : QueryParamRequest,
        IQueryBinding<ListPermissionQuery>,
        IRequest<Result<IReadOnlyList<ListGroupPermissionResponse>>>
{
    public static ValueTask<ListPermissionQuery> BindAsync(HttpContext context) =>
        ValueTask.FromResult(QueryParamRequestExtension.Bind<ListPermissionQuery>(context));
}
