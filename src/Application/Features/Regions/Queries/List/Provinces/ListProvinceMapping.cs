using Application.SharedFeatures.Projections.Regions;
using Domain.Aggregates.Regions;

namespace Application.Features.Regions.Queries.List.Provinces;

// public class ListProvinceMapping : Profile
// {
//     public ListProvinceMapping()
//     {
//         CreateMap<Province, ProvinceProjection>();
//         CreateMap<Province, ProvinceDetailProjection>();
//     }
// }

public static class ListProvinceMapping
{
    public static ProvinceProjection ToProvinceProjection(this Province province)
    {
        ProvinceProjection projection = new();
        projection.MappingFrom(province);

        return projection;
    }

    public static ProvinceDetailProjection ToProvinceDetailProjection(this Province province)
    {
        ProvinceDetailProjection projection = new();
        projection.MappingFrom(province);

        return projection;
    }
}
