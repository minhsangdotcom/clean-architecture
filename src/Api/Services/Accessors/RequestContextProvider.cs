using Application.Common.Interfaces.Services.Accessors;
using Application.Contracts.Constants;

namespace Api.Services.Accessors;

public class RequestContextProvider(IHttpContextAccessor contextAccessor) : IRequestContextProvider
{
    public HttpContext? HttpContext => contextAccessor.HttpContext;

    public string? GetRouteValue(string key) => HttpContext?.GetRouteValue(key)?.ToString();

    public string? GetHttpMethod() => HttpContext?.Request.Method;

    public string? GetRequestPath() => HttpContext?.Request.Path;

    public string? GetId() => GetRouteValue(RoutePath.Id);
}
