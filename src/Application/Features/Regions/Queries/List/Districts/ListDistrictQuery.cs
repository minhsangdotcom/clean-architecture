using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Application.SharedFeatures.Projections.Regions;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Regions.Queries.List.Districts;

public class ListDistrictQuery
    : QueryParamRequest,
        IRequest<Result<PaginationResponse<DistrictProjection>>>
{
    public static ValueTask<ListDistrictQuery> BindAsync(HttpContext context)
    {
        return ValueTask.FromResult(QueryParamRequestExtension.Bind<ListDistrictQuery>(context));
    }
}
