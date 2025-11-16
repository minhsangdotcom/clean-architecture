using Domain.Aggregates.Permissions;
using SharedKernel.Models;

namespace Application.Features.Common.Projections.Permissions;

public class PermissionProjection : BaseResponse
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Group { get; set; }

    public virtual void MappingFrom(Permission permission)
    {
        Id = permission.Id;
        Name = permission.Name;
        Description = permission.Description;
        CreatedAt = permission.CreatedAt;
        UpdatedAt = permission.UpdatedAt;
        CreatedBy = permission.CreatedBy;
        UpdatedBy = permission.UpdatedBy;
    }
}
