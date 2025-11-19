using Application.Features.Common.Mapping.Regions;
using Domain.Aggregates.Regions;

namespace Application.Features.Common.Projections.Regions;

public class ProvinceDetailProjection : ProvinceProjection
{
    public IEnumerable<DistrictDetailProjection>? Districts { get; set; }

    public sealed override void MappingFrom(Province province)
    {
        base.MappingFrom(province);
        Districts = province.Districts.ToListDistrictDetailProjection();
    }
}
