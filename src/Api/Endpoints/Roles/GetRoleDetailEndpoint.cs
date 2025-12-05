using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Features.Roles.Queries.Detail;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using static Application.Contracts.Permissions.PermissionNames;

namespace Api.Endpoints.Roles;

public class GetRoleDetailEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Router.RoleRoute.GetUpdateDelete, HandleAsync)
            .WithName(Router.RoleRoute.GetRouteName)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Get role details 🔎",
                Description = "Retrieves a role’s details, including its name and permission IDs.",
                Tags = [new OpenApiTag() { Name = Router.RoleRoute.Tags }],
            })
            .MustHaveAuthorization(
                permissions: PermissionGenerator.Generate(
                    PermissionResource.Role,
                    PermissionAction.Detail
                )
            );
    }

    private async Task<Results<Ok<ApiResponse<RoleDetailResponse>>, ProblemHttpResult>> HandleAsync(
        [FromRoute] string id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var command = new GetRoleDetailQuery(id);
        var result = await sender.Send(command, cancellationToken);
        return result.ToResult();
    }
}
