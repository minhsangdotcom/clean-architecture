using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Features.Roles.Commands.Update;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static Application.Contracts.Permissions.PermissionNames;

namespace Api.Endpoints.Roles;

public class UpdateRoleEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(Router.RoleRoute.GetUpdateDelete, HandleAsync)
            .WithTags(Router.RoleRoute.Tags)
            .AddOpenApiOperationTransformer(
                (operation, context, _) =>
                {
                    operation.Summary = "Update role 📝";
                    operation.Description =
                        "Updates a role’s name and its permissions using permission IDs.";
                    return Task.CompletedTask;
                }
            )
            .WithRequestValidation<RoleUpdateData>()
            .MustHaveAuthorization(
                permissions: PermissionGenerator.Generate(
                    PermissionResource.Role,
                    PermissionAction.Update
                )
            );
    }

    private async Task<Results<Ok<ApiResponse<UpdateRoleResponse>>, ProblemHttpResult>> HandleAsync(
        [FromRoute] string id,
        [FromBody] RoleUpdateData request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var command = new UpdateRoleCommand() { RoleId = id.ToString(), UpdateData = request };
        var result = await sender.Send(command, cancellationToken);
        return result.ToResult();
    }
}
