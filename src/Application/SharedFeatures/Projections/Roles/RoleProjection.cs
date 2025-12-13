using Application.Contracts.Dtos.Responses;
using Domain.Aggregates.Roles;

namespace Application.SharedFeatures.Projections.Roles;

public class RoleProjection : AuditableEntityResponse
{
    public string? Description { get; set; }

    public string? Name { get; set; }

    public virtual void MappingFrom(Role role)
    {
        Id = role.Id;
        Name = role.Name;
        Description = role.Description;
        CreatedAt = role.CreatedAt;
        CreatedBy = role.CreatedBy;
        UpdatedAt = role.UpdatedAt;
        UpdatedBy = role.UpdatedBy;
    }
}
