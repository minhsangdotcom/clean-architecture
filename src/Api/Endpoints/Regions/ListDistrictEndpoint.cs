using Api.common.Documents;
using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Responses;
using Application.Features.Regions.Queries.List.Districts;
using Application.SharedFeatures.Projections.Regions;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Regions;

public class ListDistrictEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Router.RegionRoute.Districts, HandleAsync)
            .WithTags(Router.RegionRoute.Tags)
            .AddOpenApiOperationTransformer(
                (operation, context, _) =>
                {
                    operation.Summary = "Get list of districts 🗺️ 🇻🇳";
                    operation.Description = "Retrieves a list of districts in Vietnam.";
                    operation.Parameters = operation.AddDocs();
                    return Task.CompletedTask;
                }
            );
    }

    private async Task<
        Results<Ok<ApiResponse<PaginationResponse<DistrictProjection>>>, ProblemHttpResult>
    > HandleAsync(
        ListDistrictQuery request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToResult();
    }
}
