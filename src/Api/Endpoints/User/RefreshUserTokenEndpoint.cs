using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.Token;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.User;

public class RefreshUserTokenEndpoint() : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Router.UserRoute.RefreshToken, HandleAsync)
            .WithTags(Router.UserRoute.Tags)
            .AddOpenApiOperationTransformer(
                (operation, context, _) =>
                {
                    operation.Summary = "Refresh Access Token 🔄 🔐";
                    operation.Description =
                        "obtains a new pair of token by providing a valid refresh token.";
                    return Task.CompletedTask;
                }
            );
    }

    private async Task<
        Results<Ok<ApiResponse<RefreshUserTokenResponse>>, ProblemHttpResult>
    > HandleAsync(
        [FromBody] RefreshUserTokenCommand request,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToResult();
    }
}
