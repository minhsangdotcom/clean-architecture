using System.Reflection;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Binds;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.QueueLogs.Queries;

public class ListQueueLogQuery
    : QueryParamRequest,
        IQueryBinding<ListQueueLogQuery>,
        IRequest<Result<PaginationResponse<ListQueueLogResponse>>>
{
    public static ValueTask<ListQueueLogQuery> BindAsync(HttpContext context) =>
        ValueTask.FromResult(QueryParamRequestExtension.Bind<ListQueueLogQuery>(context));
}
