using ByteAether.Ulid;
using SharedKernel.Entities;

namespace Domain.Common;

public class Entity : Entity<Ulid>
{
    public override Ulid Id { get; protected set; }

    protected Entity()
    {
        Id = Ulid.New();
    }

    protected Entity(Ulid id)
    {
        Id = id;
    }
}
