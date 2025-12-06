using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Features.Users.Commands.ResetPassword;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.User;

public class ResetUserPasswordEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Router.UserRoute.ResetPassword, HandleAsync)
            .WithTags(Router.UserRoute.Tags)
            .AddOpenApiOperationTransformer(
                (operation, context, _) =>
                {
                    operation.Summary = "Reset user password 🔄 🔑";
                    operation.Description =
                        "Resets a user's password using a valid token from a password reset request.";
                    return Task.CompletedTask;
                }
            )
            .WithRequestValidation<ResetUserPasswordCommand>();
    }

    private async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromBody] ResetUserPasswordCommand request,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToNoContentResult();
    }
}
