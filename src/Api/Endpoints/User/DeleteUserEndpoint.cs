using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Features.Users.Commands.Delete;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using static Contracts.Permissions.PermissionNames;

namespace Api.Endpoints.User;

public class DeleteUserEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(Router.UserRoute.GetUpdateDelete, HandleAsync)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Delete user 🗑️",
                Description = "Deletes an existing user identified by their unique ID.",
                Tags = [new OpenApiTag() { Name = Router.UserRoute.Tags }],
            })
            .RequireAuth(
                permissions: PermissionGenerator.Generate(
                    PermissionAction.Delete,
                    PermissionResource.User
                )
            );
    }

    private async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] string id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(new DeleteUserCommand(Ulid.Parse(id)), cancellationToken);
        return result.ToNoContentResult();
    }
}
