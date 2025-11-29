using Application.Features.Users.Commands.Profiles;
using Application.Features.Users.Commands.Update;
using Domain.Aggregates.Users;

namespace Application.SubcutaneousTests.Extensions;

public static class UserMappingExtension
{
    public static UpdateUserCommand ToUpdateUserCommand(User user) =>
        new()
        {
            UserId = user.Id.ToString(),
            UpdateData = new UserUpdateData()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
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
            DateOfBirth = user.DateOfBirth,
            PhoneNumber = user.PhoneNumber,
        };
}
