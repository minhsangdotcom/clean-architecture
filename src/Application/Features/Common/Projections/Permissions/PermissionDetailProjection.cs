using Domain.Aggregates.Permissions;
using Domain.Aggregates.Permissions.Enums;

namespace Application.Features.Common.Projections.Permissions;

public class PermissionDetailProjection : PermissionProjection
{
    public bool IsDeleted { get; private set; }

    public DateTimeOffset? EffectiveFrom { get; private set; }

    public DateTimeOffset? EffectiveTo { get; private set; }

    public PermissionStatus Status { get; private set; } = PermissionStatus.Active;

    public override void MappingFrom(Permission permission)
    {
        base.MappingFrom(permission);
        IsDeleted = permission.IsDeleted;
        EffectiveFrom = permission.EffectiveFrom;
        EffectiveTo = permission.EffectiveTo;
        Status = permission.Status;
    }
}
