using Domain.Aggregates.Regions;

namespace Application.SharedFeatures.Projections.Regions;

public class ProvinceProjection : Region
{
    public virtual void MappingFrom(Province province)
    {
        Code = province.Code;
        Name = province.Name;
        EnglishName = province.EnglishName;
        FullName = province.FullName;
        EnglishFullName = province.EnglishFullName;
        CustomName = province.CustomName;

        Id = province.Id;
        CreatedAt = province.CreatedAt;
        CreatedBy = province.CreatedBy;
        UpdatedAt = province.UpdatedAt;
        UpdatedBy = province.UpdatedBy;
    }
}
