using Api.common.Documents;
using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Features.Permissions;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Permissions;

public class ListPermissionEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Router.PermissionRoute.Permissions, HandleAsync)
            .WithTags(Router.PermissionRoute.Tags)
            .AddOpenApiOperationTransformer(
                (operation, context, _) =>
                {
                    operation.Summary = "Get list of Permissions in Application 📄";
                    operation.Description = "Retrieves a list of permissions in Application.";
                    operation.Parameters = operation.AddDocs();
                    return Task.CompletedTask;
                }
            );
    }

    private async Task<
        Results<Ok<ApiResponse<IReadOnlyList<ListGroupPermissionResponse>>>, ProblemHttpResult>
    > HandleAsync(
        ListPermissionQuery request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToResult();
    }
}
