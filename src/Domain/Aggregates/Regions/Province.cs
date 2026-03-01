using System.Text.Json.Serialization;
using ByteAether.Ulid;

namespace Domain.Aggregates.Regions;

public class Province : Region
{
    public override Ulid Id { get; protected set; } = Ulid.New();
    public ICollection<District> Districts { get; set; } = [];

    [JsonConstructor]
    public Province(Ulid id)
    {
        Id = id;
    }

    private Province() { }
}
