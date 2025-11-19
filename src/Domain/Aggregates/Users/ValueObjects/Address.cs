using Ardalis.GuardClauses;
using SharedKernel.Common;

namespace Domain.Aggregates.Users.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; private set; } = string.Empty;

    public Ulid ProvinceId { get; private set; }
    public string Province { get; private set; } = string.Empty;

    public Ulid DistrictId { get; private set; }
    public string District { get; private set; } = string.Empty;

    public Ulid? CommuneId { get; private set; }
    public string? Commune { get; private set; }

    private Address() { }

    public Address(
        string province,
        Ulid provinceId,
        string district,
        Ulid districtId,
        string? commune,
        Ulid? communeId,
        string street
    )
    {
        Guard.Against.StringTooLong(province, 256, nameof(province));
        Guard.Against.StringTooLong(district, 256, nameof(district));
        if (commune is not null)
        {
            Guard.Against.StringTooLong(commune, 256, nameof(commune));
        }
        Province = Guard.Against.NullOrEmpty(province);
        District = Guard.Against.NullOrEmpty(district);
        Street = Guard.Against.NullOrEmpty(street);
        ProvinceId = Guard.Against.Null(provinceId);
        DistrictId = Guard.Against.Null(districtId);
        CommuneId = communeId;
        Commune = commune;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return ProvinceId;
        yield return DistrictId;
        yield return CommuneId!;
    }

    public override bool Equals(object? obj)
    {
        if (obj is Address other)
        {
            return Street == other.Street
                && ProvinceId == other.ProvinceId
                && DistrictId == other.DistrictId
                && CommuneId == other.CommuneId;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        string commune = Commune != null ? $"{Commune}, " : string.Empty;
        return $"{Street},{commune}{District},{Province}";
    }
}
