using System.Reflection;
using Api.common.Localizations.Json;
using Api.Services.Localizations;
using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services.Localization;
using Application.Contracts.ErrorCodes;
using Application.Contracts.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace Api.Extensions;

public static class LocalizationExtension
{
    public static IServiceCollection AddLocalizationConfigurations(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
        services.AddTransient(option =>
        {
            var factory = option.GetRequiredService<IStringLocalizerFactory>();
            return factory.Create("", "");
        });
        services.AddScoped<IMessageTranslatorService, MessageTranslatorService>();
        services.AddScoped<IPermissionTranslatorService, PermissionTranslatorService>();

        LocalizationSettings localizationSettings = new();
        configuration.GetSection(nameof(LocalizationSettings)).Bind(localizationSettings);

        services.Configure<LocalizationSettings>(
            configuration.GetSection(nameof(LocalizationSettings))
        );

        string[] supportedCultures = localizationSettings.SupportedCultures;
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.SetDefaultCulture(localizationSettings.DefaultCulture);

            options.AddSupportedCultures(supportedCultures);
            options.AddSupportedUICultures(supportedCultures);
        });
        return services;
    }

    public static void AddSynchronizedLocalizationEndpoint(this WebApplication application)
    {
        application
            .MapGet(
                "/api/localizations/sync",
                (
                    [FromServices] PermissionDefinitionContext permissionDefinitionContext,
                    [FromServices] IOptions<LocalizationSettings> options
                ) =>
                {
                    IEnumerable<Type> messageTypes = typeof(UserErrorMessages)
                        .Assembly.GetTypes()
                        .Where(t =>
                            t.IsClass
                            && t.GetCustomAttribute<ErrorMessageContainerAttribute>() != null
                        );

                    foreach (Type type in messageTypes)
                    {
                        ErrorMessageLoader.LoadFromType(type);
                    }

                    List<string> errorMessages =
                    [
                        .. ErrorMessageRegistry.Messages.Select(x => x.Value).Distinct(),
                    ];

                    string baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "Resources");
                    string messageFolder = Path.Combine(baseFolder, nameof(Messages));
                    Directory.CreateDirectory(messageFolder);

                    List<string> permissionKeys =
                    [
                        .. permissionDefinitionContext.Groups.SelectMany(x =>
                        {
                            List<string> permissionCode = [x.Key];
                            permissionCode.AddRange(
                                x.Value.Permissions.Flatten().Select(x => x.Code).Distinct()
                            );
                            return permissionCode;
                        }),
                    ];
                    string permissionFolder = Path.Combine(baseFolder, nameof(Permissions));
                    Directory.CreateDirectory(permissionFolder);

                    string[] cultures = options.Value.SupportedCultures;
                    foreach (string culture in cultures)
                    {
                        string messagePath = Path.Combine(
                            messageFolder,
                            $"{nameof(Messages)}.{culture}.json"
                        );
                        TranslationFileHelper.SyncTranslationFile(messagePath, errorMessages);

                        string permissionPath = Path.Combine(
                            permissionFolder,
                            $"{nameof(Permissions)}.{culture}.json"
                        );
                        TranslationFileHelper.SyncTranslationFile(permissionPath, permissionKeys);
                    }

                    return Results.Ok();
                }
            )
            .AddOpenApiOperationTransformer(
                (operation, _, _) =>
                {
                    operation.Summary = "Synchronize localization resources ✨";
                    operation.Description =
                        "Synchronizes permissions and error messages with the localization JSON files by adding missing entries and removing obsolete ones.";
                    operation.Tags = new HashSet<OpenApiTagReference>()
                    {
                        new("Localizations-endpoint"),
                    };
                    return Task.CompletedTask;
                }
            );
    }
}
