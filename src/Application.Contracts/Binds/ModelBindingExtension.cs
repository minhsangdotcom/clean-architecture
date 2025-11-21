using Application.Contracts.Dtos.Requests;

namespace Application.Contracts.Binds;

public static class ModelBindingExtension
{
    public static string[] GetFilterQueries(string query)
    {
        string[] queryParams = query[1..].Split("&", StringSplitOptions.TrimEntries);
        return
        [
            .. queryParams.Where(param =>
                param.StartsWith(
                    nameof(QueryParamRequest.Filter),
                    StringComparison.OrdinalIgnoreCase
                )
            ),
        ];
    }
}
