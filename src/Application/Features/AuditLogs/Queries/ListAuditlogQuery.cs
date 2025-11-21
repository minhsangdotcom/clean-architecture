using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Requests;
using Mediator;
using Microsoft.AspNetCore.Http;
using SharedKernel.Models;

namespace Application.Features.AuditLogs.Queries;

public class ListAuditLogQuery
    : QueryParamRequest,
        IRequest<Result<PaginationResponse<ListAuditLogResponse>>>
{
    public static ValueTask<ListAuditLogQuery> BindAsync(HttpContext context)
    {
        return ValueTask.FromResult(QueryParamRequestExtension.Bind<ListAuditLogQuery>(context));
    }
}
