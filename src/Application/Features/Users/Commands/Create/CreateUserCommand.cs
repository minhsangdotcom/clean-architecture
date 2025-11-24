using Application.Contracts.ApiWrapper;
using Application.SharedFeatures.Requests.Users;
using Domain.Aggregates.Users.Enums;
using Mediator;

namespace Application.Features.Users.Commands.Create;

public class CreateUserCommand : UserUpsertCommand, IRequest<Result<CreateUserResponse>>
{
    public string? Username { get; set; }
    public string? Email { get; set; }

    public string? Password { get; set; }

    public Gender? Gender { get; set; }
}
