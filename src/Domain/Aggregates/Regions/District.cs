using System.Text.Json.Serialization;

namespace Domain.Aggregates.Regions;

public class District : Region
{
    public override Ulid Id { get; protected set; } = Ulid.NewUlid();

    public Ulid ProvinceId { get; set; }

    public ICollection<Commune>? Communes { get; set; } = [];

    [JsonConstructor]
    public District(Ulid id)
    {
        Id = id;
    }

    private District() { }
}
