using Api.common.Documents;
using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Responses;
using Application.Features.AuditLogs.Queries;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.AuditLogs;

public class ListAuditLogEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Router.AuditLogRoute.AuditLogs, HandleAsync)
            .WithTags(Router.AuditLogRoute.Tags)
            .AddOpenApiOperationTransformer(
                (operation, context, _) =>
                {
                    operation.Summary = "Get list of audit logs";
                    operation.Description = "Returns a list of audit logs";
                    operation.Parameters = operation.AddDocs();
                    return Task.CompletedTask;
                }
            );
    }

    private async Task<
        Results<Ok<ApiResponse<PaginationResponse<ListAuditLogResponse>>>, ProblemHttpResult>
    > HandleAsync(
        ListAuditLogQuery request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToResult();
    }
}
