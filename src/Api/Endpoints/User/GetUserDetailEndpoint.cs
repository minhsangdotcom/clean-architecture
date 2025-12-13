using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Queries.Detail;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static Application.Contracts.Permissions.PermissionNames;

namespace Api.Endpoints.User;

public class GetUserDetailEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Router.UserRoute.GetUpdateDelete, HandleAsync)
            .WithName(Router.UserRoute.GetRouteName)
            .WithTags(Router.UserRoute.Tags)
            .AddOpenApiOperationTransformer(
                (operation, context, _) =>
                {
                    operation.Summary = "Get user by ID 🧾";
                    operation.Description =
                        "Retrieves detailed information of a user based on their unique ID.";
                    return Task.CompletedTask;
                }
            )
            .MustHaveAuthorization(
                permissions: PermissionGenerator.Generate(
                    PermissionResource.User,
                    PermissionAction.Detail
                )
            );
    }

    private async Task<
        Results<Ok<ApiResponse<GetUserDetailResponse>>, ProblemHttpResult>
    > HandleAsync(
        [FromRoute] string id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var command = new GetUserDetailQuery(id);
        var result = await sender.Send(command, cancellationToken);
        return result.ToResult();
    }
}
