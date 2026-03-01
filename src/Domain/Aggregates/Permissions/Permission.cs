using Domain.Common;

namespace Domain.Aggregates.Permissions;

public class Permission : AuditableEntity
{
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? Group { get; private set; }

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
