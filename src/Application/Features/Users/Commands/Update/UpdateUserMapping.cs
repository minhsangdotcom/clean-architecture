using Domain.Aggregates.Users;

namespace Application.Features.Users.Commands.Update;

public static class UpdateUserMapping
{
    public static User FromUpdateUser(this User user, UserUpdateData update)
    {
        return user.Update(
            update.FirstName!,
            update.LastName!,
            update.PhoneNumber,
            update.DateOfBirth
        );
    }

    public static UpdateUserResponse ToUpdateUserResponse(this User user)
    {
        var response = new UpdateUserResponse();
        response.MappingFrom(user);

        return response;
    }
}
