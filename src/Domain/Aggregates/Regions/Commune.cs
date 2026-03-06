using System.Text.Json.Serialization;
using ByteAether.Ulid;

namespace Domain.Aggregates.Regions;

public class Commune : Region
{
    public override Ulid Id { get; protected set; } = Ulid.New();
    public Ulid DistrictId { get; set; }

    public District? District { get; set; }

    [JsonConstructor]
    public Commune(Ulid id)
    {
        Id = id;
    }

    private Commune() { }
}
