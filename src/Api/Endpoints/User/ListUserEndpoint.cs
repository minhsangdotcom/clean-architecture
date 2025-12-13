using Api.common.Documents;
using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Responses;
using Application.Features.Users.Queries.List;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static Application.Contracts.Permissions.PermissionNames;

namespace Api.Endpoints.User;

public class ListUserEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Router.UserRoute.Users, HandleAsync)
            .WithTags(Router.UserRoute.Tags)
            .AddOpenApiOperationTransformer(
                (operation, context, _) =>
                {
                    operation.Summary = "Get list of user 📄";
                    operation.Description =
                        "Retrieves a list of all registered users in the system.";
                    operation.Parameters = operation.AddDocs();
                    return Task.CompletedTask;
                }
            )
            .MustHaveAuthorization(
                permissions: PermissionGenerator.Generate(
                    PermissionResource.User,
                    PermissionAction.List
                )
            );
    }

    private async Task<
        Results<Ok<ApiResponse<PaginationResponse<ListUserResponse>>>, ProblemHttpResult>
    > HandleAsync(
        ListUserQuery request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToResult();
    }
}
