using Application.Contracts.Dtos.Responses;
using Application.SharedFeatures.Projections.Permissions;

namespace Application.Features.Permissions;

public class ListGroupPermissionResponse
{
    public string? Name { get; set; }
    public string? NameTranslation { get; set; }
    public List<ListPermissionResponse>? Permissions { get; set; }
}

public class ListPermissionResponse : PermissionResponse
{
    public List<ListPermissionResponse>? Children { get; set; }
}

public class PermissionResponse : EntityResponse
{
    public string? Code { get; set; }
    public string? CodeTranslation { get; set; }
}
