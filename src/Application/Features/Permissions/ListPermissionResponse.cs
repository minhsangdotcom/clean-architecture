using Application.Features.Common.Projections.Permissions;

namespace Application.Features.Permissions;

public class ListPermissionResponse : PermissionResponse
{
    public IReadOnlyCollection<PermissionResponse>? Children { get; set; }
}

public class PermissionResponse : PermissionProjection;
