using Application.SharedFeatures.Projections.Users;

namespace Application.Features.Users.Commands.Login;

public class LoginUserResponse : UserTokenProjection
{
    public UserProjection? User { get; set; }
}
