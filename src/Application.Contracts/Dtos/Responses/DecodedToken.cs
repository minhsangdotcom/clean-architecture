using System.Text.Json.Serialization;
using ClaimTypes = SharedKernel.Constants.ClaimTypes;

namespace Application.Contracts.Dtos.Responses;

public class DecodedToken
{
    [JsonPropertyName(ClaimTypes.Sub)]
    public string? Sub { get; set; }

    [JsonPropertyName(ClaimTypes.TokenFamilyId)]
    public string? FamilyId { get; set; }
};
