using SharedKernel.Models;

namespace Domain.Common;

public abstract class Entity : Entity<Ulid>
{
    public override Ulid Id { get; protected set; } = Ulid.NewUlid();
}

public abstract class Entity<T>
{
    public virtual T Id { get; protected set; } = default!;

    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
}

public abstract class AuditableEntity : AuditableEntity<Ulid>
{
    public override Ulid Id { get; protected set; } = Ulid.NewUlid();
}

public abstract class AuditableEntity<T> : Entity<T>, IAuditable
{
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public interface IAuditable : IBaseAuditable;
