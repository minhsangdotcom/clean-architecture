using SharedKernel.Models;

namespace Domain.Common;

public abstract class DefaultEntity
{
    public Ulid Id { get; protected set; } = Ulid.NewUlid();

    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
}

public abstract class DefaultEntity<T>
{
    public virtual T Id { get; protected set; } = default!;

    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
}

public abstract class BaseEntity : DefaultEntity, IAuditable
{
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public abstract class BaseEntity<T> : DefaultEntity<T>, IAuditable
{
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public interface IAuditable : IBaseAuditable;
