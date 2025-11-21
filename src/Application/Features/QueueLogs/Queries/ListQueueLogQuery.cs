using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Requests;
using Mediator;
using Microsoft.AspNetCore.Http;
using SharedKernel.Models;

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
