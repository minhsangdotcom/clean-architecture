using Domain.Aggregates.Users;

namespace Application.Features.Users.Queries.Profiles;

public static class GetUserProfileMapping
{
    public static GetUserProfileResponse ToGetUserProfileResponse(this User user)
    {
        var response = new GetUserProfileResponse();
        response.MappingFrom(user);

        return response;
    }
}
