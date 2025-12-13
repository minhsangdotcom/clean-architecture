using Application.SubcutaneousTests.Dtos.Requests;
using Application.SubcutaneousTests.Dtos.Responses;
using Application.SubcutaneousTests.Extensions;
using Infrastructure.Constants;

namespace Application.SubcutaneousTests;

public partial class TestingFixture
{
    private string? cachedToken;

    public async Task<HttpResponseMessage> MakeRequestAsync(
        string uri,
        HttpMethod method,
        object? payload = null,
        string? contentType = "application/json"
    )
    {
        if (client == null)
        {
            throw new InvalidOperationException($"{nameof(client)} is null.");
        }
        string token = await EnsureAuthTokenAsync(client);

        return await client.SendAsync(
            new HttpTestRequest()
            {
                Uri = $"{BASE_URL}{uri}",
                Method = method,
                Payload = payload,
                ContentType = contentType!,
                Token = token,
            }
        );
    }

    private async Task<string> EnsureAuthTokenAsync(HttpClient client)
    {
        if (!string.IsNullOrEmpty(cachedToken))
        {
            return cachedToken!;
        }

        var loginPayload = new
        {
            Username = "super.admin",
            Password = Credential.USER_DEFAULT_PASSWORD,
        };

        HttpResponseMessage response = await client.SendAsync(
            new HttpTestRequest()
            {
                Uri = $"{BASE_URL}users/login",
                Method = HttpMethod.Post,
                Payload = loginPayload,
            }
        );

        var loginResponse = await response.ToResponse<Response<LoginResponse>>();
        if (loginResponse?.Results?.Token == null)
        {
            throw new Exception("Login failed — cannot obtain JWT token.");
        }

        cachedToken = loginResponse.Results.Token;
        return cachedToken!;
    }
}
