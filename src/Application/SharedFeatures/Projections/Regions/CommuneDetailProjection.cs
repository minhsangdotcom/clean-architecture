using Application.Features.Regions.Queries.List.Districts;
using Domain.Aggregates.Regions;

namespace Application.SharedFeatures.Projections.Regions;

public class CommuneDetailProjection : CommuneProjection
{
    public DistrictProjection? District { get; set; }

    public override void MappingFrom(Commune commune)
    {
        base.MappingFrom(commune);
        District = commune.District?.ToDistrictProjection();
    }
}
