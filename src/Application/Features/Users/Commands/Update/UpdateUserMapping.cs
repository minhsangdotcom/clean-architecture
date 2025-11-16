using Domain.Aggregates.Users;

namespace Application.Features.Users.Commands.Update;

public static class UpdateUserMapping
{
    public static User FromUpdateUser(this User user, UserUpdateRequest update)
    {
        return user.FromMapping(
            update.FirstName,
            update.LastName,
            update.Email,
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
