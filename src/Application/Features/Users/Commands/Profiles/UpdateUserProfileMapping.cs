using Domain.Aggregates.Users;

namespace Application.Features.Users.Commands.Profiles;

// public class UpdateUserProfileMapping : Profile
// {
//     public UpdateUserProfileMapping()
//     {
//         CreateMap<UpdateUserProfileCommand, User>();
//         CreateMap<User, UpdateUserProfileResponse>();
//     }
// }

public static class UpdateUserProfileMapping
{
    public static User MapFromUpdateUserProfileCommand(
        this User user,
        UpdateUserProfileCommand profileCommand
    )
    {
        return user.FromMapping(
            profileCommand.FirstName,
            profileCommand.LastName,
            profileCommand.Email,
            profileCommand.PhoneNumber,
            profileCommand.DateOfBirth
        );
    }

    public static UpdateUserProfileResponse ToUpdateUserProfileResponse(this User user)
    {
        var response = new UpdateUserProfileResponse();
        response.MappingFrom(user);

        return response;
    }
}
