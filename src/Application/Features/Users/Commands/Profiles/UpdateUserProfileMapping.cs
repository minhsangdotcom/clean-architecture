using Domain.Aggregates.Users;

namespace Application.Features.Users.Commands.Profiles;

public static class UpdateUserProfileMapping
{
    public static User MapFromCommand(this User user, UpdateUserProfileCommand command)
    {
        return user.UpdateProfile(
            command.FirstName!,
            command.LastName!,
            command.Gender,
            command.PhoneNumber,
            command.DateOfBirth
        );
    }

    public static UpdateUserProfileResponse ToUpdateUserProfileResponse(this User user)
    {
        var response = new UpdateUserProfileResponse();
        response.MappingFrom(user);

        return response;
    }
}
