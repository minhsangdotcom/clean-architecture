using Application.SharedFeatures.Projections.Regions;
using Domain.Aggregates.Regions;

namespace Application.SharedFeatures.Mapping.Regions;

public static class RegionsMapping
{
    public static CommuneProjection ToCommuneProjection(this Commune? commune)
    {
        if (commune == null)
        {
            return null!;
        }
        var response = new CommuneProjection();
        response.MappingFrom(commune);
        return response;
    }

    public static IReadOnlyList<CommuneProjection> ToListCommuneProjection(
        this IEnumerable<Commune>? communes
    )
    {
        if (communes == null)
        {
            return [];
        }
        return [.. communes.Select(c => c.ToCommuneProjection())];
    }

    public static CommuneDetailProjection ToCommuneDetailProjection(this Commune commune)
    {
        if (commune == null)
        {
            return null!;
        }
        var response = new CommuneDetailProjection();
        response.MappingFrom(commune);
        return response;
    }

    public static IReadOnlyList<CommuneDetailProjection> ToListCommuneDetailProjection(
        this IEnumerable<Commune>? communes
    )
    {
        if (communes == null)
        {
            return [];
        }
        return [.. communes.Select(c => c.ToCommuneDetailProjection())];
    }

    public static DistrictDetailProjection ToDistrictDetailProjection(this District district)
    {
        if (district == null)
        {
            return null!;
        }
        var response = new DistrictDetailProjection();
        response.MappingFrom(district);
        return response;
    }

    public static IReadOnlyList<DistrictDetailProjection> ToListDistrictDetailProjection(
        this IEnumerable<District>? districts
    )
    {
        if (districts == null)
        {
            return [];
        }
        return [.. districts.Select(c => c.ToDistrictDetailProjection())];
    }
}
