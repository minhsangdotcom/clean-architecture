using Application.Contracts.Dtos.Responses;

namespace Application.SharedFeatures.Projections.Roles;

public class RoleClaimDetailProjection : EntityResponse
{
    public string? ClaimType { get; set; }

    public string? ClaimValue { get; set; }
}
