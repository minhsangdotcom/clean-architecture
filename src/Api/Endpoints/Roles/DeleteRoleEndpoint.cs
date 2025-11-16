using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Features.Roles.Commands.Delete;
using Contracts.ApiWrapper;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using static Contracts.Permissions.PermissionNames;

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
                Description =
                    "Deletes an existing role by its unique ID. Once deleted, the role and its associated claims/permission will no longer be available",
                Tags = [new OpenApiTag() { Name = Router.RoleRoute.Tags }],
            })
            .RequireAuth(
                permissions: Permission.Generate(PermissionAction.Delete, PermissionResource.Role)
            );
    }

    private async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] string id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(new DeleteRoleCommand(Ulid.Parse(id)), cancellationToken);
        return result.ToNoContentResult();
    }
}
