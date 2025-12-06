using Api.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Api.Extensions;

public static class OpenApiExtension
{
    public static IServiceCollection AddOpenApiConfiguration(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        OpenApiSettings? openApiSettings = configuration
            .GetSection(nameof(OpenApiSettings))
            .Get<OpenApiSettings>();

        return services.AddOpenApi(
            "v1",
            options =>
            {
                options.AddDocumentTransformer(
                    (document, context, cancellationToken) =>
                    {
                        document.Info = new()
                        {
                            Title = $"{openApiSettings?.ApplicationName} Documentation",
                            Version = "v1",
                            Description =
                                $"Well come to the {openApiSettings?.ApplicationName} API",
                            Contact = new OpenApiContact()
                            {
                                Name = openApiSettings?.Name,
                                Email = openApiSettings?.Email,
                            },
                        };
                        return Task.CompletedTask;
                    }
                );
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            }
        );
    }
}

internal sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider
) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (
            authenticationSchemes.Any(authScheme =>
                authScheme.Name == JwtBearerDefaults.AuthenticationScheme
            )
        )
        {
            var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                [JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token",
                    Description = "Please enter token",
                    Name = "Authorization",
                },
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = securitySchemes;

            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations!))
            {
                operation.Value.Security ??= [];
                operation.Value.Security.Add(
                    new OpenApiSecurityRequirement
                    {
                        [
                            new OpenApiSecuritySchemeReference(
                                JwtBearerDefaults.AuthenticationScheme,
                                document
                            )
                        ] = [],
                    }
                );
            }
        }
    }
}
