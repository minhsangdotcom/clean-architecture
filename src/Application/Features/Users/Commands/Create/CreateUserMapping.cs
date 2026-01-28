using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;

namespace Application.Features.Users.Commands.Create;

public static class CreateUserMapping
{
    public static User ToUser(this CreateUserCommand command)
    {
        User user = new(
            command.FirstName!,
            command.LastName!,
            command.Username!,
            command.Password!,
            command.Email!,
            command.PhoneNumber!,
            command.DateOfBirth,
            command.Gender
        );
        if (command.Status == UserStatus.Inactive)
        {
            user.Deactivate();
        }
        return user;
    }

    public static CreateUserResponse ToCreateUserResponse(this User user)
    {
        CreateUserResponse response = new();
        response.MappingFrom(user);
        return response;
    }
}
