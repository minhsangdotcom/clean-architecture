using System.Text.Json.Serialization;
using ClaimTypes = SharedKernel.Constants.ClaimTypes;
using SecurityClaimTypes = System.Security.Claims.ClaimTypes;

namespace Application.Contracts.Dtos.Responses;

public class DecodedToken
{
    [JsonPropertyName(SecurityClaimTypes.NameIdentifier)]
    public string? Sub { get; set; }

    [JsonPropertyName(ClaimTypes.TokenFamilyId)]
    public string? FamilyId { get; set; }
};
