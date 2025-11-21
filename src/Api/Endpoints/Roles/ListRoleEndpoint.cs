using Api.common.Documents;
using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Features.Roles.Queries.List;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using static Application.Contracts.Permissions.PermissionNames;

namespace Api.Endpoints.Roles;

public class ListRoleEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Router.RoleRoute.Roles, HandleAsync)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Get list of roles 📋",
                Description =
                    "Retrieves a list of all available roles in the system, along with their basic information (e.g., name, assigned permissions, etc.).",
                Tags = [new OpenApiTag() { Name = Router.RoleRoute.Tags }],
                Parameters = operation.AddDocs(),
            })
            .RequireAuth(
                permissions: PermissionGenerator.Generate(
                    PermissionAction.List,
                    PermissionResource.Role
                )
            );
    }

    private async Task<
        Results<Ok<ApiResponse<IReadOnlyList<ListRoleResponse>>>, ProblemHttpResult>
    > HandleAsync(
        ListRoleQuery request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToResult();
    }
}
