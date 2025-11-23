using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Requests;
using Application.Contracts.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.QueueLogs.Queries;

public class ListQueueLogQuery
    : QueryParamRequest,
        IRequest<Result<PaginationResponse<ListQueueLogResponse>>>
{
    public static ValueTask<ListQueueLogQuery> BindAsync(HttpContext context)
    {
        return ValueTask.FromResult(QueryParamRequestExtension.Bind<ListQueueLogQuery>(context));
    }
}
