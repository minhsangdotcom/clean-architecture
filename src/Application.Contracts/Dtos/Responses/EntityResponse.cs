namespace Application.Contracts.Dtos.Responses;

public class EntityResponse<T>
{
    public T Id { get; set; } = default!;
    public DateTimeOffset? CreatedAt { get; set; }
}

public class EntityResponse : EntityResponse<Ulid>;

public class AuditableEntityResponse<T> : EntityResponse<T>
{
    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}

public class AuditableEntityResponse : AuditableEntityResponse<Ulid>;
