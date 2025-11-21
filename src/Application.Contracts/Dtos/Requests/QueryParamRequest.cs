using Application.Contracts.Binds;
using Microsoft.AspNetCore.Http;

namespace Application.Contracts.Dtos.Requests;

public class QueryParamRequest
{
    /// <summary>
    /// The current page
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Maximum items that display per page
    /// </summary>
    public int PageSize { get; set; } = 100;

    /// <summary>
    /// Cursor pagination
    /// </summary>
    public string? Before { get; set; }

    public string? After { get; set; }

    public string? Keyword { get; set; }

    /// <summary>
    /// Fields want to search for
    /// </summary>
    public List<string>? Targets { get; set; }

    /// <summary>
    /// example : Sort=Age:desc,Name:asc
    /// default is asc
    /// </summary>
    public string? Sort { get; set; }

    public object? Filter { get; set; } = null;

    public string[]? OriginFilters { get; set; }
}

public static class QueryParamRequestExtension
{
    public static T Bind<T>(HttpContext context)
        where T : QueryParamRequest, new()
    {
        string? before = context.Request.Query["before"];
        string? after = context.Request.Query["after"];
        string? keyword = context.Request.Query["keyword"];
        string? sort = context.Request.Query["sort"];
        if (!int.TryParse(context.Request.Query["page"], out int page))
        {
            page = 1;
        }
        if (!int.TryParse(context.Request.Query["pageSize"], out int pageSize))
        {
            pageSize = 100;
        }

        string[] queryString = GetQueryParams(context);
        var query = context.Request.Query["targets"];
        var targets = query.Count > 0 ? query.ToList() : null;
        return new()
        {
            Page = page,
            PageSize = pageSize,
            Before = before,
            After = after,
            Keyword = keyword,
            Sort = sort,
            OriginFilters = queryString,
            Targets = targets!,
        };
    }

    private static string[] GetQueryParams(HttpContext httpContext)
    {
        string? queryStringValue = httpContext?.Request.QueryString.Value;

        if (string.IsNullOrEmpty(queryStringValue))
        {
            return [];
        }

        return ModelBindingExtension.GetFilterQueries(queryStringValue);
    }
}
