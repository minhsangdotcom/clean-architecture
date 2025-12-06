using System.Text.Json.Nodes;
using Microsoft.OpenApi;

namespace Api.common.Documents;

public static class QueryParamRequestDocument
{
    public static IList<IOpenApiParameter> AddDocs(this OpenApiOperation _)
    {
        return
        [
            // page
            new OpenApiParameter()
            {
                Name = "page",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Integer,
                    Default = JsonValue.Create(1),
                },
            },
            // pageSize
            new OpenApiParameter()
            {
                Name = "pageSize",
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Integer,
                    Default = JsonValue.Create(100),
                },
            },
            // before
            new OpenApiParameter()
            {
                Name = "before",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Description = "The cursor for the previous move",
                },
            },
            // after
            new OpenApiParameter()
            {
                Name = "after",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Description = "The cursor for the next move",
                },
            },
            // keyword
            new OpenApiParameter()
            {
                Name = "keyword",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
            },
            // targets[]
            new OpenApiParameter()
            {
                Name = "targets",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema { Type = JsonSchemaType.String },
                },
            },
            // sort
            new OpenApiParameter()
            {
                Name = "sort",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Example = JsonValue.Create("createdAt:desc,id"),
                },
            },
            // filter (complex example)
            new OpenApiParameter()
            {
                Name = "filter",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object,
                    AdditionalPropertiesAllowed = true,
                    Description =
                        "query string like : filter[$and][0][gender][$eq]=1&filter[$and][1][dayOfBirth][$between][0]=2002-10-01&filter[$and][1][dayOfBirth][$between][1]=2005-10-01",
                    Example = new JsonObject
                    {
                        ["$and"] = new JsonObject
                        {
                            ["gender"] = new JsonObject { ["$eq"] = JsonValue.Create(1) },
                            ["dateOfBirth"] = new JsonObject
                            {
                                ["$between"] = new JsonArray
                                {
                                    JsonValue.Create("2002-10-01"),
                                    JsonValue.Create("2005-10-01"),
                                },
                            },
                        },
                    },
                },
            },
        ];
    }
}
