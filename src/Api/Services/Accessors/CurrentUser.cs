using System.Security.Claims;
using Application.Common.Interfaces.Services.Accessors;

namespace Api.Services.Accessors;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Ulid? Id => GetId(ClaimTypes.NameIdentifier);

    public string? ClientIp =>
        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    private Ulid? GetId(string claimType)
    {
        string? id = httpContextAccessor.HttpContext?.User?.FindFirstValue(claimType);

        if (id is null)
        {
            return Ulid.Empty;
        }

        return Ulid.Parse(id);
    }
}
