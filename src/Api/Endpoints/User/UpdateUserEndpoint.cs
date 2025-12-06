using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.Update;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static Application.Contracts.Permissions.PermissionNames;

namespace Api.Endpoints.User;

public class UpdateUserEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(Router.UserRoute.GetUpdateDelete, HandleAsync)
            .WithTags(Router.UserRoute.Tags)
            .AddOpenApiOperationTransformer(
                (operation, context, _) =>
                {
                    operation.Summary = " Update user ✏️ 🧑‍💻";
                    operation.Description =
                        "Updates the information of an existing user identified by their ID.";
                    return Task.CompletedTask;
                }
            )
            .WithRequestValidation<UserUpdateData>()
            .MustHaveAuthorization(
                permissions: PermissionGenerator.Generate(
                    PermissionResource.User,
                    PermissionAction.Update
                )
            )
            .DisableAntiforgery();
    }

    private async Task<Results<Ok<ApiResponse<UpdateUserResponse>>, ProblemHttpResult>> HandleAsync(
        [FromRoute] string id,
        [FromForm] UserUpdateData request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var command = new UpdateUserCommand() { UserId = id, UpdateData = request };
        var result = await sender.Send(command, cancellationToken);
        return result.ToResult();
    }
}
