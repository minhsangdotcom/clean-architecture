using Api.common.Documents;
using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Features.QueueLogs.Queries;
using Contracts.ApiWrapper;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using SharedKernel.Models;

namespace Api.Endpoints.QueueLogs;

public class ListQueueLogEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Router.UserRoute.Users, HandleAsync)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Get list of Queue logging 📄",
                Description = "Retrieves a list of all logs of queue.",
                Tags = [new OpenApiTag() { Name = Router.UserRoute.Tags }],
                Parameters = operation.AddDocs(),
            })
            .RequireAuth(
                permissions: Permission.Generate(PermissionAction.List, PermissionResource.QueueLog)
            );
    }

    private async Task<
        Results<Ok<ApiResponse<PaginationResponse<ListQueueLogResponse>>>, ProblemHttpResult>
    > HandleAsync(
        ListQueueLogQuery request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToResult();
    }
}
