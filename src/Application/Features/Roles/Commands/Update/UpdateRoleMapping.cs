using Domain.Aggregates.Roles;

namespace Application.Features.Roles.Commands.Update;

public static class UpdateRoleMapping
{
    public static void FromCommand(this Role role, UpdateRoleCommand command)
    {
        var roleRequest = command.UpdateData;
        role.Update(roleRequest.Name!, roleRequest.Description);
    }

    public static UpdateRoleResponse ToUpdateRoleResponse(this Role role)
    {
        UpdateRoleResponse roleResponse = new();
        roleResponse.MappingFrom(role);
        return roleResponse;
    }
}
