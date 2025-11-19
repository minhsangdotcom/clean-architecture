using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Roles.Queries.Detail;

public record GetRoleDetailQuery(string Id) : IRequest<Result<RoleDetailResponse>>;
