using Application.Features.Roles.Commands.Update;
using Domain.Aggregates.Roles;

namespace Application.Features.Roles.Commands.Create;

public static class CreateRoleMapping
{
    public static Role ToRole(this CreateRoleCommand command) =>
        new(command.Name!, command.Description);

    public static CreateRoleResponse ToCreateRoleResponse(this Role role)
    {
        CreateRoleResponse roleResponse = new();
        roleResponse.MappingFrom(role);
        return roleResponse;
    }
}
