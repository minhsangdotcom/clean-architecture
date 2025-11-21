using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Requests;
using Mediator;
using Microsoft.AspNetCore.Http;
using SharedKernel.Models;

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
