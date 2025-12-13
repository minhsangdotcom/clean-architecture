using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Features.Roles.Commands.Create;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static Application.Contracts.Permissions.PermissionNames;

namespace Api.Endpoints.Roles;

public class CreateRoleEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Router.RoleRoute.Roles, HandleAsync)
            .WithTags(Router.RoleRoute.Tags)
            .AddOpenApiOperationTransformer(
                (operation, context, _) =>
                {
                    operation.Summary = "Create role 👮";
                    operation.Description = "Creates a new role and assigns permission IDs.";
                    return Task.CompletedTask;
                }
            )
            .WithRequestValidation<CreateRoleCommand>()
            .MustHaveAuthorization(
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
