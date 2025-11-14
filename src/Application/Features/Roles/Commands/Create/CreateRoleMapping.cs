using Application.Features.Common.Mapping.Roles;
using CaseConverter;
using Domain.Aggregates.Roles;

namespace Application.Features.Roles.Commands.Create;

public static class CreateRoleMapping
{
    public static Role ToRole(this CreateRoleCommand command) =>
        new(command.Name, command.Description);

    // {
    //     RoleClaims = roleCommand.RoleClaims?.ToListRoleClaim(),
    // };

    public static CreateRoleResponse ToCreateRoleResponse(this Role role)
    {
        CreateRoleResponse roleResponse = new();
        roleResponse.MappingFrom(role);
        return roleResponse;
    }
}
