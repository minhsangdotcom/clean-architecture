using System.Text.Json.Serialization;
using Application.Contracts.Dtos.Responses;
using Domain.Aggregates.Users.Enums;

namespace Application.Features.Common.Projections.Users;

public class UserClaimDetailProjection : EntityResponse
{
    public string? ClaimType { get; set; }

    public string? ClaimValue { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public UserClaimType Type { get; set; }
}
