using System.Collections;
using System.Reflection;
using Application.Common.Interfaces.Services.Storage;
using Application.Common.Security;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Responses;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors;

public class ProcessImagePathBehavior<TMessage, TResponse>(
    ILogger<ProcessImagePathBehavior<TMessage, TResponse>> logger,
    IStorageService storageService
) : MessagePostProcessor<TMessage, TResponse>
    where TMessage : notnull, IMessage
    where TResponse : notnull
{
    protected override ValueTask Handle(
        TMessage message,
        TResponse response,
        CancellationToken cancellationToken
    )
    {
        Type responseType = typeof(TResponse);
        if (
            !responseType.IsGenericType
            || responseType.GetGenericTypeDefinition() != typeof(Result<>)
        )
        {
            return default!;
        }

        object? value = ResultTypeHelper.ExtractValue(response);
        if (value == null)
        {
            return default!;
        }

        // Check if the response is a PaginationResponse and handle accordingly
        Type resultType = responseType.GetGenericArguments()[0];
        if (
            resultType.IsGenericType
            && resultType.GetGenericTypeDefinition() == typeof(PaginationResponse<>)
        )
        {
            ProcessPaginationResponse(value);
            return default!;
        }

        // Handle non-pagination responses
        ProcessSingleResponse(value);
        return default!;
    }

    // Processes responses of type PaginationResponse<>
    private void ProcessPaginationResponse(object response)
    {
        PropertyInfo? dataProperty = response
            .GetType()
            .GetProperty(nameof(PaginationResponse<object>.Data));
        if (dataProperty == null)
        {
            return;
        }

        object? dataPropertyValue = dataProperty.GetValue(response);
        if (dataPropertyValue is IEnumerable dataEnumerable)
        {
            foreach (object data in dataEnumerable)
            {
                ProcessDataPropertiesWithFileAttribute(data);
            }
        }
    }

    // Processes individual response properties with the [File] attribute
    private void ProcessSingleResponse(object response) =>
        ProcessDataPropertiesWithFileAttribute(response);

    // Processes the properties of a data object within a pagination response
    private void ProcessDataPropertiesWithFileAttribute(object data)
    {
        IEnumerable<PropertyInfo> propertiesWithFileAttribute = GetFileAttributeProperties(
            data.GetType()
        );

        foreach (PropertyInfo prop in propertiesWithFileAttribute)
        {
            object? imageKey = prop.GetValue(data);
            if (imageKey == null)
            {
                continue;
            }

            logger.LogInformation("image key {value}", imageKey);

            UpdatePropertyIfNotPublicUrl(data, prop, imageKey);
        }
    }

    // Updates the property value if the key does not already have http url
    private void UpdatePropertyIfNotPublicUrl(object target, PropertyInfo property, object key)
    {
        string imageKeyStr = key.ToString()!;
        if (!imageKeyStr.StartsWith(storageService.PublicUrl))
        {
            string? fullPath = storageService.GetFullPath(imageKeyStr);
            string? publicPath = storageService.GetPublicPath(fullPath!);
            logger.LogInformation("image path {value}", publicPath);
            property.SetValue(target, fullPath);
        }
    }

    // Retrieves all properties with the [File] attribute from the given type
    private static IEnumerable<PropertyInfo> GetFileAttributeProperties(Type type) =>
        type.GetProperties()
            .Where(prop =>
                prop.CustomAttributes.Any(attr => attr.AttributeType == typeof(FileAttribute))
            );
}
