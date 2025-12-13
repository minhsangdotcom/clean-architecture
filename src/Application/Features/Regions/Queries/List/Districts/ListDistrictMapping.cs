using Application.SharedFeatures.Projections.Regions;
using Domain.Aggregates.Regions;

namespace Application.Features.Regions.Queries.List.Districts;

public static class ListDistrictMapping
{
    public static DistrictProjection ToDistrictProjection(this District district)
    {
        DistrictProjection districtProjection = new();
        districtProjection.MappingFrom(district);

        return districtProjection;
    }

    public static DistrictDetailProjection ToDistrictDetailProjection(this District district)
    {
        DistrictDetailProjection districtProjection = new();
        districtProjection.MappingFrom(district);

        return districtProjection;
    }
}
