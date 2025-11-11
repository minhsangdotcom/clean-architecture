using System.Collections;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using DotNetCoreExtension.Extensions;
using DotNetCoreExtension.Extensions.Reflections;
using Microsoft.AspNetCore.Http;

namespace Application.SubcutaneousTests.Extensions;

public static class ClientExtension
{
    public static async Task<T?> ToResponse<T>(this HttpResponseMessage responseMessage) =>
        await responseMessage.Content.ReadFromJsonAsync<T>();

    public static async Task<HttpResponseMessage> CreateRequestAsync(
        this HttpClient client,
        string uriString,
        HttpMethod method,
        object payload,
        string? contentType = null,
        string? token = null
    )
    {
        Uri uri = new(uriString);
        HttpContent content =
            contentType == "multipart/form-data"
                ? CreateMultipartFormDataContent(payload)
                : new StringContent(
                    SerializerExtension.Serialize(payload).StringJson,
                    Encoding.UTF8,
                    contentType ?? "application/json"
                );
        using HttpRequestMessage httpRequest =
            new()
            {
                Method = method,
                RequestUri = uri,
                Content = content,
            };
        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                token
            );
        }
        return await client.SendAsync(httpRequest);
    }

    public static MultipartFormDataContent CreateMultipartFormDataContent(object obj)
    {
        var multipartContent = new MultipartFormDataContent();

        foreach (var property in obj.GetType().GetProperties())
        {
            var propertyName = property.Name;
            var propertyValue = property.GetValue(obj);

            if (propertyValue == null)
            {
                continue;
            }

            if (propertyValue is IFormFile formFile)
            {
                var streamContent = new StreamContent(formFile.OpenReadStream());
                streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(
                    "multipart/form-data"
                );
                multipartContent.Add(streamContent, propertyName, formFile.FileName);
                continue;
            }

            if (
                propertyValue != null
                && typeof(IEnumerable).IsAssignableFrom(property.PropertyType)
                && property.PropertyType != typeof(string)
            )
            {
                Type genericType = property.PropertyType.GetGenericArguments()[0];
                IList list = (IList)propertyValue;
                if (genericType.IsUserDefineType())
                {
                    foreach (var item in list)
                    {
                        var properties = item.GetType().GetProperties();
                        for (int i = 0; i < properties.Length; i++)
                        {
                            var prop = properties[i];
                            string name = prop.Name;
                            object? value = prop.GetValue(item);

                            if (value != null)
                            {
                                multipartContent.Add(
                                    new StringContent(
                                        value.ToString()!,
                                        Encoding.UTF8,
                                        MediaTypeNames.Text.Plain
                                    ),
                                    $"{propertyName}[{i}].{name}"
                                );
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        object? value = list[i];

                        if (value != null)
                        {
                            multipartContent.Add(
                                new StringContent(
                                    value?.ToString()!,
                                    Encoding.UTF8,
                                    MediaTypeNames.Text.Plain
                                ),
                                $"{propertyName}[{i}]"
                            );
                        }
                    }
                }
                continue;
            }

            multipartContent.Add(
                new StringContent(
                    propertyValue!.ToString()!,
                    Encoding.UTF8,
                    MediaTypeNames.Text.Plain
                ),
                propertyName
            );
        }

        return multipartContent;
    }
}
