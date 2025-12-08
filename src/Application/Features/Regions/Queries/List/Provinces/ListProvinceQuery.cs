using Application.Contracts.ApiWrapper;
using Application.Contracts.Binds;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Application.SharedFeatures.Projections.Regions;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Regions.Queries.List.Provinces;

public class ListProvinceQuery
    : QueryParamRequest,
        IQueryBinding<ListProvinceQuery>,
        IRequest<Result<PaginationResponse<ProvinceProjection>>>
{
    public static ValueTask<ListProvinceQuery> BindAsync(HttpContext context) =>
        ValueTask.FromResult(QueryParamRequestExtension.Bind<ListProvinceQuery>(context));
}
