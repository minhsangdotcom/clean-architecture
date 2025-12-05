using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Features.Roles.Commands.Delete;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using static Application.Contracts.Permissions.PermissionNames;

namespace Api.Endpoints.Roles;

public class DeleteRoleEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(Router.RoleRoute.GetUpdateDelete, HandleAsync)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = " Delete role 🗑️",
                Description = "Deletes a role by its ID along with its permissions.",
                Tags = [new OpenApiTag() { Name = Router.RoleRoute.Tags }],
            })
            .MustHaveAuthorization(
                permissions: PermissionGenerator.Generate(
                    PermissionResource.Role,
                    PermissionAction.Delete
                )
            );
    }

    private async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] string id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(new DeleteRoleCommand(id), cancellationToken);
        return result.ToNoContentResult();
    }
}
