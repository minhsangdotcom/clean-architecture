using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Features.Roles.Commands.Update;
using Contracts.ApiWrapper;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using static Contracts.Permissions.PermissionNames;

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
                Description =
                    "Updates an existing role's information. You can modify the name and add or remove claims/permissions. This endpoint helps ensure your authorization model stays current with your users' needs.",
                Tags = [new OpenApiTag() { Name = Router.RoleRoute.Tags }],
            })
            .WithRequestValidation<UpdateRoleRequest>()
            .RequireAuth(
                permissions: Permission.Generate(PermissionAction.Update, PermissionResource.Role)
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
