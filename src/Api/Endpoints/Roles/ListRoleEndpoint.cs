using Api.common.Documents;
using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Features.Roles.Queries.List;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static Application.Contracts.Permissions.PermissionNames;

namespace Api.Endpoints.Roles;

public class ListRoleEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Router.RoleRoute.Roles, HandleAsync)
            .WithTags(Router.RoleRoute.Tags)
            .AddOpenApiOperationTransformer(
                (operation, context, _) =>
                {
                    operation.Summary = "Get list of roles 📋";
                    operation.Description = "Retrieves all roles with their basic information.";
                    operation.Parameters = operation.AddDocs();
                    return Task.CompletedTask;
                }
            )
            .MustHaveAuthorization(
                permissions: PermissionGenerator.Generate(
                    PermissionResource.Role,
                    PermissionAction.List
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
