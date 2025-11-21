using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Requests;
using Application.Features.Common.Projections.Regions;
using Mediator;
using Microsoft.AspNetCore.Http;
using SharedKernel.Models;

namespace Application.Features.Regions.Queries.List.Communes;

public class ListCommuneQuery
    : QueryParamRequest,
        IRequest<Result<PaginationResponse<CommuneProjection>>>
{
    public static ValueTask<ListCommuneQuery> BindAsync(HttpContext context)
    {
        return ValueTask.FromResult(QueryParamRequestExtension.Bind<ListCommuneQuery>(context));
    }
}
