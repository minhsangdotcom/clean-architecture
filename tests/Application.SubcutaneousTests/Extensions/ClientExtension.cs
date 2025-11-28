using System.Collections;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using Application.SubcutaneousTests.Dtos.Requests;
using DotNetCoreExtension.Extensions;
using DotNetCoreExtension.Extensions.Reflections;
using Microsoft.AspNetCore.Http;

namespace Application.SubcutaneousTests.Extensions;

public static class ClientExtension
{
    public static async Task<T?> ToResponse<T>(this HttpResponseMessage responseMessage) =>
        await responseMessage.Content.ReadFromJsonAsync<T>();

    public static async Task<HttpResponseMessage> SendAsync(
        this HttpClient client,
        HttpTestRequest request
    )
    {
        ArgumentNullException.ThrowIfNull(client);

        using HttpRequestMessage httpRequest =
            new()
            {
                Method = request.Method,
                RequestUri = new Uri(request.Uri),
                Content = CreateContent(request),
            };

        // Add Bearer token per request (not globally on client)
        if (!string.IsNullOrWhiteSpace(request.Token))
        {
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                request.Token
            );
        }

        return await client.SendAsync(httpRequest);
    }

    private static HttpContent CreateContent(HttpTestRequest req)
    {
        if (req.Payload == null)
        {
            return new StringContent(string.Empty);
        }

        if (req.ContentType == "multipart/form-data")
        {
            return CreateMultipartFormData(req.Payload);
        }

        // JSON or other structured type
        string json = SerializerExtension.Serialize(req.Payload).StringJson;
        return new StringContent(json, Encoding.UTF8, req.ContentType ?? "application/json");
    }

    private static MultipartFormDataContent CreateMultipartFormData(object payload)
    {
        MultipartFormDataContent content = [];
        Type type = payload.GetType();

        foreach (PropertyInfo prop in type.GetProperties())
        {
            string name = prop.Name;
            object? value = prop.GetValue(payload);

            if (value == null)
            {
                continue;
            }

            if (value is IFormFile formFile)
            {
                AddFile(content, name, formFile);
                continue;
            }

            if (
                typeof(IEnumerable).IsAssignableFrom(prop.PropertyType)
                && prop.PropertyType != typeof(string)
            )
            {
                var propertyType = prop.PropertyType.GetGenericArguments()[0];
                AddEnumerable(content, propertyType, (IList)value);
                continue;
            }

            content.Add(
                new StringContent(value!.ToString()!, Encoding.UTF8, MediaTypeNames.Text.Plain),
                name
            );
        }

        return content;
    }

    private static void AddFile(MultipartFormDataContent content, string name, IFormFile file)
    {
        var stream = new StreamContent(file.OpenReadStream());
        stream.Headers.ContentType = MediaTypeHeaderValue.Parse(
            file.ContentType ?? "application/octet-stream"
        );

        content.Add(stream, name, file.FileName);
    }

    private static void AddEnumerable(MultipartFormDataContent content, Type type, IList list)
    {
        if (type.IsUserDefineType())
        {
            AddComplexObject(content, type.Name, list);
        }
        else
        {
            AddPrimitiveList(content, type.Name, list);
        }
    }

    private static void AddComplexObject(
        MultipartFormDataContent content,
        string parentName,
        IList list
    )
    {
        for (int i = 0; i < list.Count; i++)
        {
            object? item = list[i];
            if (item == null)
            {
                continue;
            }
            var properties = item.GetType().GetProperties();
            foreach (var prop in properties)
            {
                string name = prop.Name;
                object? value = prop.GetValue(item);
                if (value == null)
                {
                    continue;
                }
                content.Add(
                    new StringContent(value.ToString()!, Encoding.UTF8, MediaTypeNames.Text.Plain),
                    $"{parentName}[{i}].{name}"
                );
            }
        }
    }

    private static void AddPrimitiveList(
        MultipartFormDataContent content,
        string parentName,
        IList list
    )
    {
        for (int i = 0; i < list.Count; i++)
        {
            object? value = list[i];
            if (value == null)
            {
                continue;
            }

            content.Add(
                new StringContent(value.ToString()!, Encoding.UTF8, MediaTypeNames.Text.Plain),
                $"{parentName}[{i}]"
            );
        }
    }
}
