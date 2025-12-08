using Application.Contracts.ApiWrapper;
using Application.Contracts.Binds;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Application.SharedFeatures.Projections.Regions;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Regions.Queries.List.Communes;

public class ListCommuneQuery
    : QueryParamRequest,
        IQueryBinding<ListCommuneQuery>,
        IRequest<Result<PaginationResponse<CommuneProjection>>>
{
    public static ValueTask<ListCommuneQuery> BindAsync(HttpContext context) =>
        ValueTask.FromResult(QueryParamRequestExtension.Bind<ListCommuneQuery>(context));
}
