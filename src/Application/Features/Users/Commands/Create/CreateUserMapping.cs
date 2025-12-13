using Domain.Aggregates.Users;

namespace Application.Features.Users.Commands.Create;

public static class CreateUserMapping
{
    public static User ToUser(this CreateUserCommand command) =>
        new(
            command.FirstName!,
            command.LastName!,
            command.Username!,
            command.Password!,
            command.Email!,
            command.PhoneNumber!,
            command.DateOfBirth,
            command.Gender
        );

    public static CreateUserResponse ToCreateUserResponse(this User user)
    {
        CreateUserResponse response = new();
        response.MappingFrom(user);
        return response;
    }
}
