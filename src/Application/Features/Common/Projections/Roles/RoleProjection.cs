using Domain.Aggregates.Roles;
using SharedKernel.Models;

namespace Application.Features.Common.Projections.Roles;

public class RoleProjection : BaseResponse
{
    public string? Description { get; set; }

    public string? Name { get; set; }

    public virtual void MappingFrom(Role role)
    {
        Id = role.Id;
        CreatedAt = role.CreatedAt;
        Name = role.Name;
        Description = role.Description;
    }
}
