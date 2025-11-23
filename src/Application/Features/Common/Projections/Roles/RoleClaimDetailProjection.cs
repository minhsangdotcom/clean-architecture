using Application.Contracts.Dtos.Responses;

namespace Application.Features.Common.Projections.Roles;

public class RoleClaimDetailProjection : EntityResponse
{
    public string? ClaimType { get; set; }

    public string? ClaimValue { get; set; }
}
