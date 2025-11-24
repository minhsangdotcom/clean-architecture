using Domain.Aggregates.Regions;

namespace Application.SharedFeatures.Projections.Regions;

public class DistrictProjection : Region
{
    public Ulid ProvinceId { get; set; }

    public virtual void MappingFrom(District district)
    {
        Code = district.Code;
        Name = district.Name;
        EnglishName = district.EnglishName;
        FullName = district.FullName;
        EnglishFullName = district.EnglishFullName;
        CustomName = district.CustomName;
        ProvinceId = district.ProvinceId;

        Id = district.Id;
        CreatedAt = district.CreatedAt;
        CreatedBy = district.CreatedBy;
        UpdatedAt = district.UpdatedAt;
        UpdatedBy = district.UpdatedBy;
    }
}
