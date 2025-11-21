using Application.Common.Interfaces.Services;
using Application.Contracts.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Infrastructure.Services;

public class HttpContextAccessorService(IHttpContextAccessor httpContextAccessor)
    : IHttpContextAccessorService
{
    public HttpContext? HttpContext => httpContextAccessor.HttpContext;

    public string? GetRouteValue(string key) => HttpContext?.GetRouteValue(key)?.ToString();

    public string? GetHttpMethod() => HttpContext?.Request.Method;

    public string? GetRequestPath() => HttpContext?.Request.Path;

    public string? GetId() => GetRouteValue(RoutePath.Id);
}
