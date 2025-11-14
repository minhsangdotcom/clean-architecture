using Microsoft.AspNetCore.Http;

namespace Application.Features.Common.Payloads.Users;

public class UserPayload
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public IFormFile? Avatar { get; set; }
}
