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
using Microsoft.OpenApi.Models;
using static Application.Contracts.Permissions.PermissionNames;

namespace Api.Endpoints.User;

public class ListUserEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Router.UserRoute.Users, HandleAsync)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Get list of user 📄",
                Description = "Retrieves a list of all registered users in the system.",
                Tags = [new OpenApiTag() { Name = Router.UserRoute.Tags }],
                Parameters = operation.AddDocs(),
            })
            .Authorize(
                permissions: PermissionGenerator.Generate(
                    PermissionAction.List,
                    PermissionResource.User
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
