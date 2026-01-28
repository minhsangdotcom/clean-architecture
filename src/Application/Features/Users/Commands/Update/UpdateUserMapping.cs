using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;

namespace Application.Features.Users.Commands.Update;

public static class UpdateUserMapping
{
    public static User FromUpdateUser(this User user, UserUpdateData update)
    {
        user.Update(
            update.FirstName!,
            update.LastName!,
            update.Email!,
            update.PhoneNumber,
            update.DateOfBirth
        );

        if (update.Status == UserStatus.Inactive)
        {
            user.Deactivate();
        }
        return user;
    }

    public static UpdateUserResponse ToUpdateUserResponse(this User user)
    {
        var response = new UpdateUserResponse();
        response.MappingFrom(user);

        return response;
    }
}
