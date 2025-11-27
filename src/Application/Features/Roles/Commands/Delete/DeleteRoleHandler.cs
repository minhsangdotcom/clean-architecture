using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Localization;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Domain.Aggregates.Roles;
using Mediator;

namespace Application.Features.Roles.Commands.Delete;

public class DeleteRoleHandler(IRoleManager manager, IMessageTranslatorService translator)
    : IRequestHandler<DeleteRoleCommand, Result<string>>
{
    public async ValueTask<Result<string>> Handle(
        DeleteRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        Role? role = await manager.FindByIdAsync(
            Ulid.Parse(command.RoleId),
            false,
            cancellationToken
        );
        if (role == null)
        {
            return Result<string>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    new(
                        RoleErrorMessages.RoleNotFound,
                        translator.Translate(RoleErrorMessages.RoleNotFound)
                    )
                )
            );
        }

        await manager.DeleteAsync(role, cancellationToken);
        return Result<string>.Success();
    }
}
