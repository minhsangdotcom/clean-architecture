using Application.SharedFeatures.Projections.Permissions;

namespace Application.Features.Permissions;

public class ListGroupPermissionResponse
{
    public string? GroupName { get; set; }
    public List<ListPermissionResponse>? Permissions { get; set; }
}

public class ListPermissionResponse : PermissionResponse
{
    public List<ListPermissionResponse>? Children { get; set; }
}

public class PermissionResponse : PermissionProjection;
