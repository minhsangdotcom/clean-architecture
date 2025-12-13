using Application.Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Users.Queries.Detail;

public record GetUserDetailQuery(string UserId) : IRequest<Result<GetUserDetailResponse>>;
