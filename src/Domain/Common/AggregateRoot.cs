using ByteAether.Ulid;
using SharedKernel.DomainEvents;
using SharedKernel.Entities;

namespace Domain.Common;

public abstract class AggregateRoot : AggregateRoot<Ulid>
{
    protected AggregateRoot()
    {
        Id = Ulid.New();
    }

    protected AggregateRoot(Ulid id)
    {
        Id = id;
    }
}
