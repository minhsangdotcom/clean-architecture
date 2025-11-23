using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Users.Queries.List;

public class ListUserQuery
    : QueryParamRequest,
        IRequest<Result<PaginationResponse<ListUserResponse>>>
{
    public static ValueTask<ListUserQuery> BindAsync(HttpContext context)
    {
        return ValueTask.FromResult(QueryParamRequestExtension.Bind<ListUserQuery>(context));
    }
}
