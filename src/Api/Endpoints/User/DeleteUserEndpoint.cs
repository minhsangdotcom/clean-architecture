using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Features.Users.Commands.Delete;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static Application.Contracts.Permissions.PermissionNames;

namespace Api.Endpoints.User;

public class DeleteUserEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(Router.UserRoute.GetUpdateDelete, HandleAsync)
            .WithTags(Router.UserRoute.Tags)
            .AddOpenApiOperationTransformer(
                (operation, context, _) =>
                {
                    operation.Summary = "Delete user 🗑️";
                    operation.Description =
                        "Deletes an existing user identified by their unique ID.";
                    return Task.CompletedTask;
                }
            )
            .MustHaveAuthorization(
                permissions: PermissionGenerator.Generate(
                    PermissionResource.User,
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
        var result = await sender.Send(new DeleteUserCommand(id), cancellationToken);
        return result.ToNoContentResult();
    }
}
