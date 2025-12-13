using System.Text.Json.Serialization;

namespace Domain.Aggregates.Regions;

public class Province : Region
{
    public override Ulid Id { get; protected set; } = Ulid.NewUlid();
    public ICollection<District> Districts { get; set; } = [];

    [JsonConstructor]
    public Province(Ulid id)
    {
        Id = id;
    }

    private Province() { }
}
