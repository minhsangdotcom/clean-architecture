using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Requests;
using Application.Features.Common.Projections.Regions;
using Mediator;
using Microsoft.AspNetCore.Http;
using SharedKernel.Models;

namespace Application.Features.Regions.Queries.List.Provinces;

public class ListProvinceQuery
    : QueryParamRequest,
        IRequest<Result<PaginationResponse<ProvinceProjection>>>
{
    public static ValueTask<ListProvinceQuery> BindAsync(HttpContext context)
    {
        return ValueTask.FromResult(QueryParamRequestExtension.Bind<ListProvinceQuery>(context));
    }
}
