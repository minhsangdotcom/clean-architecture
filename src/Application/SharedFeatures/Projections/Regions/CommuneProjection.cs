using Domain.Aggregates.Regions;

namespace Application.SharedFeatures.Projections.Regions;

public class CommuneProjection : Region
{
    public Ulid DistrictId { get; set; }

    public virtual void MappingFrom(Commune commune)
    {
        Id = commune.Id;
        Code = commune.Code;
        Name = commune.Name;
        EnglishName = commune.EnglishName;
        FullName = commune.FullName;
        EnglishFullName = commune.EnglishFullName;
        CustomName = commune.CustomName;
        DistrictId = commune.DistrictId;

        CreatedAt = commune.CreatedAt;
        CreatedBy = commune.CreatedBy;
        UpdatedAt = commune.UpdatedAt;
        UpdatedBy = commune.UpdatedBy;
    }
}
