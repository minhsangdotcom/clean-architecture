using Application.Features.Common.Requests.Users;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Users.Commands.Profiles;

public class UpdateUserProfileCommand : UserUpsertCommand, IRequest<Result<UpdateUserProfileResponse>>;
