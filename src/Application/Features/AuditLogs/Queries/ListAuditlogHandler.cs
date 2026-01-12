using Application.Common.Interfaces.Services.Elasticsearch;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Responses;
using Domain.Aggregates.AuditLogs;
using Mediator;

namespace Application.Features.AuditLogs.Queries;

public class ListAuditLogHandler(IElasticsearchServiceFactory? factory = null)
    : IRequestHandler<ListAuditLogQuery, Result<PaginationResponse<ListAuditLogResponse>>>
{
    public async ValueTask<Result<PaginationResponse<ListAuditLogResponse>>> Handle(
        ListAuditLogQuery request,
        CancellationToken cancellationToken
    )
    {
        if (factory == null)
        {
            throw new NotImplementedException("Elasticsearch has not enabled");
        }

        List<AuditLog> auditLogs = await factory.Get<AuditLog>().ListAsync(request);
        PaginationResponse<ListAuditLogResponse> paginationResponse = new(
            auditLogs.ToListAuditLogResponse(),
            auditLogs.Count,
            request.Page,
            request.PageSize
        );

        return Result<PaginationResponse<ListAuditLogResponse>>.Success(paginationResponse);
    }
}
