using System.Data.Common;
using Application.Common.Interfaces.Services.Accessors;
using Application.Contracts.Permissions;
using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace Application.SubcutaneousTests;

public class CustomWebApplicationFactory<TProgram>(
    DbConnection dbConnection,
    string environmentName,
    IConfiguration configuration
) : WebApplicationFactory<TProgram>
    where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<TheDbContext>)
            );
            services.Remove(descriptor!);

            var dbConnectionDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbConnection)
            );
            services.Remove(dbConnectionDescriptor!);

            services.AddDbContext<TheDbContext>(
                (container, options) => options.UseNpgsql(dbConnection)
            );

            services
                .RemoveAll<ICurrentUser>()
                .AddTransient(provider =>
                    Mock.Of<ICurrentUser>(x => x.Id == TestingFixture.GetUserId())
                );

            services
                .RemoveAll<PermissionDefinitionContext>()
                .AddSingleton(_ =>
                {
                    PermissionDefinitionContext context = new();
                    new SystemPermissionDefinitionProvider().Define(context);
                    return context;
                });
        });

        builder.UseEnvironment(environmentName).UseConfiguration(configuration);

        string? rootPath = FindProjectRoot("Application.SubcutaneousTests.csproj");
        if (!string.IsNullOrEmpty(rootPath))
        {
            builder.UseContentRoot(rootPath);
        }
    }

    static string? FindProjectRoot(string projectFileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir != null && !File.Exists(Path.Combine(dir.FullName, projectFileName)))
        {
            dir = dir.Parent;
        }

        return dir?.FullName;
    }
}
