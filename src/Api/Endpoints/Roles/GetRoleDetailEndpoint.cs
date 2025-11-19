using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Features.Roles.Queries.Detail;
using Contracts.ApiWrapper;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using static Contracts.Permissions.PermissionNames;

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
                Description =
                    "Retrieves detailed information about a specific role, including its name and associated claims/permissions. Use this to review or audit the role’s configurations.",
                Tags = [new OpenApiTag() { Name = Router.RoleRoute.Tags }],
            })
            .RequireAuth(
                permissions: PermissionGenerator.Generate(
                    PermissionAction.Detail,
                    PermissionResource.Role
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
