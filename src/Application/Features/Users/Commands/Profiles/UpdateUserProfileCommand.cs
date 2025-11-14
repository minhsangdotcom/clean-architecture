using Application.Features.Common.Payloads.Users;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Users.Commands.Profiles;

public class UpdateUserProfileCommand : UserPayload, IRequest<Result<UpdateUserProfileResponse>>;
