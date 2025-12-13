using SharedKernel.Entities;

namespace Domain.Aggregates.Permissions;

public class Permission : AuditableEntity
{
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? Group { get; private set; }

    // public DateTimeOffset? EffectiveFrom { get; private set; }

    // public DateTimeOffset? EffectiveTo { get; private set; }

    // public PermissionStatus Status { get; private set; } = PermissionStatus.Active;

    private Permission() { }

    public Permission(
        string code,
        string name,
        string? description = null,
        string? group = null,
        string? createdBy = null
    )
    {
        Code = code;
        Name = name;
        Description = description;
        Group = group;

        if (!string.IsNullOrWhiteSpace(createdBy))
        {
            CreatedBy = createdBy!;
        }
    }
}
