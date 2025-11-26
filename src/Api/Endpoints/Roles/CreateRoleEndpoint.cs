using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Features.Roles.Commands.Create;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using static Application.Contracts.Permissions.PermissionNames;

namespace Api.Endpoints.Roles;

public class CreateRoleEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Router.RoleRoute.Roles, HandleAsync)
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary = "Create role 👮",
                Description = "Creates a new role and assigns permission IDs.",
                Tags = [new OpenApiTag() { Name = Router.RoleRoute.Tags }],
            })
            .WithRequestValidation<CreateRoleCommand>()
            .Authorize(
                permissions: PermissionGenerator.Generate(
                    PermissionResource.Role,
                    PermissionAction.Create
                )
            );
    }

    private async Task<
        Results<CreatedAtRoute<ApiResponse<CreateRoleResponse>>, ProblemHttpResult>
    > HandleAsync(
        [FromBody] CreateRoleCommand request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToCreatedResult(result.Value!.Id, Router.RoleRoute.GetRouteName);
    }
}
