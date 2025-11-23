using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Features.Roles.Commands.Update;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using static Application.Contracts.Permissions.PermissionNames;

namespace Api.Endpoints.Roles;

public class UpdateRoleEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(Router.RoleRoute.GetUpdateDelete, HandleAsync)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Update role 📝",
                Description = "Updates a role’s name and its permissions using permission IDs.",
                Tags = [new OpenApiTag() { Name = Router.RoleRoute.Tags }],
            })
            .WithRequestValidation<UpdateRoleRequest>()
            .Authorize(
                permissions: PermissionGenerator.Generate(
                    PermissionAction.Update,
                    PermissionResource.Role
                )
            );
    }

    private async Task<Results<Ok<ApiResponse<UpdateRoleResponse>>, ProblemHttpResult>> HandleAsync(
        [FromRoute] string id,
        [FromBody] UpdateRoleRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var command = new UpdateRoleCommand() { RoleId = id.ToString(), UpdateData = request };
        var result = await sender.Send(command, cancellationToken);
        return result.ToResult();
    }
}
