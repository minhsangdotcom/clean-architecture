using Application.Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Users.Queries.Profiles;

public class GetUserProfileQuery() : IRequest<Result<GetUserProfileResponse>>;
