using Application.Features.Roles.Commands.Update;
using Domain.Aggregates.Roles;

namespace Application.SubcutaneousTests.Extensions;

public static class RoleMappingExtension
{
    public static UpdateRoleCommand ToUpdateRoleCommand(Role role)
    {
        return new()
        {
            RoleId = role.Id.ToString(),
            UpdateData = new RoleUpdateData()
            {
                Name = role.Name,
                Description = role.Description,
            },
        };
    }
}
