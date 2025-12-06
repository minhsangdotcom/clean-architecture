using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Queries.Profiles;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.User;

public class GetUserProfileEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Router.UserRoute.Profile, HandleAsync)
            .WithTags(Router.UserRoute.Tags)
            .AddOpenApiOperationTransformer(
                (operation, context, _) =>
                {
                    operation.Summary = "Get current user's profile 🧑‍💼";
                    operation.Description = "Returns user profile if found";
                    return Task.CompletedTask;
                }
            )
            .MustHaveAuthorization();
    }

    private async Task<
        Results<Ok<ApiResponse<GetUserProfileResponse>>, ProblemHttpResult>
    > HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetUserProfileQuery(), cancellationToken);
        return result!.ToResult();
    }
}
