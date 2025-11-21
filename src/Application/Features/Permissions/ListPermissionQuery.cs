using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Requests;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Permissions;

public class ListPermissionQuery
    : QueryParamRequest,
        IRequest<Result<IReadOnlyList<ListGroupPermissionResponse>>>
{
    public static ValueTask<ListPermissionQuery> BindAsync(HttpContext context)
    {
        return ValueTask.FromResult(QueryParamRequestExtension.Bind<ListPermissionQuery>(context));
    }
}
