using Application.Common.Interfaces.Services.Elasticsearch;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Responses;
using Domain.Aggregates.AuditLogs;
using Mediator;

namespace Application.Features.AuditLogs.Queries;

public class ListAuditLogHandler(IElasticsearchServiceFactory? elasticsearch = null)
    : IRequestHandler<ListAuditLogQuery, Result<PaginationResponse<ListAuditLogResponse>>>
{
    public async ValueTask<Result<PaginationResponse<ListAuditLogResponse>>> Handle(
        ListAuditLogQuery request,
        CancellationToken cancellationToken
    )
    {
        if (elasticsearch == null)
        {
            throw new NotImplementedException("Elasticsearch has not enabled");
        }

        IList<AuditLog> auditLogs = await elasticsearch.Get<AuditLog>().ListAsync(request);
        PaginationResponse<ListAuditLogResponse> paginationResponse =
            new(
                auditLogs.ToListAuditLogResponse(),
                auditLogs.Count,
                request.Page,
                request.PageSize
            );

        return Result<PaginationResponse<ListAuditLogResponse>>.Success(paginationResponse);
    }
}
