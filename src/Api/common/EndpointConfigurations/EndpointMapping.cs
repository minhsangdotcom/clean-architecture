using System.Reflection;
using Api.common.Routers;
using Application.Contracts.Constants;
using Asp.Versioning;
using Asp.Versioning.Builder;

namespace Api.common.EndpointConfigurations;

public static class EndpointMapping
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        return services.Scan(scan =>
            scan.FromAssemblies(assembly)
                .AddClasses(classes => classes.AssignableTo<IEndpoint>())
                .As<IEndpoint>()
                .WithSingletonLifetime()
        );
    }

    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        EndpointVersion apiVersion
    )
    {
        int version = (int)apiVersion;
        ApiVersionSet apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(version))
            .ReportApiVersions()
            .Build();

        List<IEndpoint> endpoints =
        [
            .. app
                .Services.GetRequiredService<IEnumerable<IEndpoint>>()
                .Where(endpoint => endpoint.Version == apiVersion),
        ];

        RouteGroupBuilder v1 = app.MapGroup($"/{RoutePath.prefix}" + "v{version:apiVersion}/")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(version);

        endpoints.ForEach(endpoint => endpoint.MapEndpoint(v1));
        return app;
    }
}
