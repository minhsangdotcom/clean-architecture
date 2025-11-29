using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Api.common.Documents;

public static class QueryParamRequestDocument
{
    public static IList<OpenApiParameter> AddDocs(this OpenApiOperation _)
    {
        return
        [
            new()
            {
                Name = "page",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new() { Type = "integer", Default = new OpenApiInteger(1) },
            },
            new()
            {
                Name = "pageSize",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new() { Type = "integer", Default = new OpenApiInteger(100) },
            },
            new()
            {
                Name = "before",
                In = ParameterLocation.Query,
                Schema = new()
                {
                    Type = "string",
                    Description = "The cursor for the previous move",
                },
            },
            new()
            {
                Name = "after",
                In = ParameterLocation.Query,
                Schema = new() { Type = "string", Description = "The cursor for the next move" },
            },
            new()
            {
                Name = "keyword",
                In = ParameterLocation.Query,
                Schema = new() { Type = "string" },
            },
            new()
            {
                Name = "targets",
                In = ParameterLocation.Query,
                Schema = new()
                {
                    Type = "array",
                    Items = new() { Type = "string" },
                },
            },
            new()
            {
                Name = "sort",
                In = ParameterLocation.Query,
                Schema = new()
                {
                    Type = "string",
                    Example = new OpenApiString("createdAt:desc, id"),
                },
            },
            new()
            {
                Name = "filter",
                In = ParameterLocation.Query,
                Schema = new()
                {
                    Type = "object",
                    AdditionalPropertiesAllowed = true,
                    Description =
                        "query string like : filter[$and][0][gender][$eq]=1&filter[$and][1][dayOfBirth][$between][0]=2002-10-01&filter[$and][1][dayOfBirth][$between][1]=2005-10-01",
                    Example = new OpenApiObject(
                        new()
                        {
                            ["$and"] = new OpenApiObject()
                            {
                                ["gender"] = new OpenApiObject()
                                {
                                    ["$eq"] = new OpenApiInteger(1),
                                },
                                ["dateOfBirth"] = new OpenApiObject()
                                {
                                    ["$between"] = new OpenApiArray()
                                    {
                                        new OpenApiDate(new DateTime(2002, 10, 1)),
                                        new OpenApiDate(new DateTime(2005, 10, 1)),
                                    },
                                },
                            },
                        }
                    ),
                },
            },
        ];
    }
}
