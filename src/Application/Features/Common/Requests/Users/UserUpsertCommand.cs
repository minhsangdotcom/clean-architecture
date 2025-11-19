using Domain.Aggregates.Users.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Common.Requests.Users;

public class UserUpsertCommand
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public IFormFile? Avatar { get; set; }

    public UserStatus Status { get; set; }

    public List<Ulid>? Roles { get; set; }

    public List<Ulid>? Permissions { get; set; }
}
