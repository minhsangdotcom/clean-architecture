using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces.Services.Accessors;

public interface IRequestContextProvider
{
    HttpContext? HttpContext { get; }
    string? GetRouteValue(string key);
    string? GetHttpMethod();
    string? GetRequestPath();
    string? GetId();
}
