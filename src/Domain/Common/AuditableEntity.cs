using ByteAether.Ulid;
using SharedKernel.Entities;

namespace Domain.Common;

public class AuditableEntity : AuditableEntity<Ulid>
{
    protected AuditableEntity()
    {
        Id = Ulid.New();
    }

    protected AuditableEntity(Ulid id)
    {
        Id = id;
    }
}
