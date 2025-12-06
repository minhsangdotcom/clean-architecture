using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.Profiles;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.User;

public class UpdateUserProfileEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(Router.UserRoute.Profile, HandleAsync)
            .WithTags(Router.UserRoute.Tags)
            .AddOpenApiOperationTransformer(
                (operation, context, _) =>
                {
                    operation.Summary = "Update user profile 🛠️ 👨 📋";
                    operation.Description =
                        "Updates profile information for the currently authenticated user.";
                    return Task.CompletedTask;
                }
            )
            .WithRequestValidation<UpdateUserProfileCommand>()
            .MustHaveAuthorization()
            .DisableAntiforgery();
    }

    private async Task<
        Results<Ok<ApiResponse<UpdateUserProfileResponse>>, ProblemHttpResult>
    > HandleAsync(
        [FromForm] UpdateUserProfileCommand request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToResult();
    }
}
