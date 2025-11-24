using Application.Contracts.ApiWrapper;
using Application.SharedFeatures.Requests.Users;
using Mediator;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserCommand : IRequest<Result<UpdateUserResponse>>
{
    public string UserId { get; set; } = string.Empty;

    public UserUpdateRequest UpdateData { get; set; } = null!;
}

public class UserUpdateRequest : UserUpsertCommand;
