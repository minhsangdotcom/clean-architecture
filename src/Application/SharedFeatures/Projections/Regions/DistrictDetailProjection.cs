using Application.SharedFeatures.Mapping.Regions;
using Domain.Aggregates.Regions;

namespace Application.SharedFeatures.Projections.Regions;

public class DistrictDetailProjection : DistrictProjection
{
    public IEnumerable<CommuneProjection>? Communes { get; set; }

    public sealed override void MappingFrom(District district)
    {
        base.MappingFrom(district);
        Communes = district.Communes?.ToListCommuneProjection();
    }
}
