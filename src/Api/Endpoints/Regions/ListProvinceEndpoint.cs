using Api.common.Documents;
using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Responses;
using Application.Features.Regions.Queries.List.Provinces;
using Application.SharedFeatures.Projections.Regions;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Regions;

public class ListProvinceEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Router.RegionRoute.Provinces, HandleAsync)
            .WithTags(Router.RegionRoute.Tags)
            .AddOpenApiOperationTransformer(
                (operation, context, _) =>
                {
                    operation.Summary = "Get list of provinces 🗺️ 🇻🇳";
                    operation.Description = "Retrieves a list of provinces in Vietnam.";
                    operation.Parameters = operation.AddDocs();
                    return Task.CompletedTask;
                }
            );
    }

    private async Task<
        Results<Ok<ApiResponse<PaginationResponse<ProvinceProjection>>>, ProblemHttpResult>
    > HandleAsync(
        ListProvinceQuery request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToResult();
    }
}
