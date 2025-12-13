using Application.Contracts.Dtos.Requests;
using Microsoft.AspNetCore.Http;

namespace Application.Contracts.Binds;

public interface IQueryBinding<T>
    where T : QueryParamRequest, new()
{
    public static abstract ValueTask<T> BindAsync(HttpContext context);
}
