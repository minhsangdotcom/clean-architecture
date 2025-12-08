using Application.Contracts.ApiWrapper;
using Application.Contracts.Binds;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.AuditLogs.Queries;

public class ListAuditLogQuery
    : QueryParamRequest,
        IQueryBinding<ListAuditLogQuery>,
        IRequest<Result<PaginationResponse<ListAuditLogResponse>>>
{
    public static ValueTask<ListAuditLogQuery> BindAsync(HttpContext context) =>
        ValueTask.FromResult(QueryParamRequestExtension.Bind<ListAuditLogQuery>(context));
}
