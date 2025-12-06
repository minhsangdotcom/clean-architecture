using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.Create;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static Application.Contracts.Permissions.PermissionNames;

namespace Api.Endpoints.User;

public class CreateUserEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Router.UserRoute.Users, HandleAsync)
            .WithTags(Router.UserRoute.Tags)
            .AddOpenApiOperationTransformer(
                (operation, context, _) =>
                {
                    operation.Summary = "Create user 🧑";
                    operation.Description =
                        "Creates a new user and returns the created user details.";
                    return Task.CompletedTask;
                }
            )
            .WithRequestValidation<CreateUserCommand>()
            .MustHaveAuthorization(
                permissions: PermissionGenerator.Generate(
                    PermissionResource.User,
                    PermissionAction.Create
                )
            )
            .DisableAntiforgery();
    }

    private async Task<
        Results<CreatedAtRoute<ApiResponse<CreateUserResponse>>, ProblemHttpResult>
    > HandleAsync(
        [FromForm] CreateUserCommand request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToCreatedResult(result.Value!.Id, Router.UserRoute.GetRouteName);
    }
}
