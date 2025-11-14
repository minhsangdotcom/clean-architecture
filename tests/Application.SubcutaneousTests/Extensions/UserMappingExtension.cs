using Application.Features.Common.Payloads.Users;
using Application.Features.Common.Projections.Users;
using Application.Features.Users.Commands.Profiles;
using Application.Features.Users.Commands.Update;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;

namespace Application.SubcutaneousTests.Extensions;

public static class UserMappingExtension
{
    public static UpdateUserCommand ToUpdateUserCommand(User user) =>
        new()
        {
            UserId = user.Id.ToString(),
            UpdateData = new UserUpdateRequest()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                PhoneNumber = user.PhoneNumber,

                Roles = [.. user.Roles!.Select(x => x.RoleId)],
            },
        };

    public static UpdateUserProfileCommand ToUpdateUserProfileCommand(User user) =>
        new()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            PhoneNumber = user.PhoneNumber,
        };
}
