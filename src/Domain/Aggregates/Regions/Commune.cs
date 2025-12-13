using System.Text.Json.Serialization;

namespace Domain.Aggregates.Regions;

public class Commune : Region
{
    public override Ulid Id { get; protected set; } = Ulid.NewUlid();
    public Ulid DistrictId { get; set; }

    public District? District { get; set; }

    [JsonConstructor]
    public Commune(Ulid id)
    {
        Id = id;
    }

    private Commune() { }
}
