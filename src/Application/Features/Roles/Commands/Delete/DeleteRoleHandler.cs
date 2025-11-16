using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Contracts.ApiWrapper;
using Domain.Aggregates.Roles;
using Mediator;
using SharedKernel.Common.Messages;

namespace Application.Features.Roles.Commands.Delete;

public class DeleteRoleHandler(IRoleManager manager)
    : IRequestHandler<DeleteRoleCommand, Result<string>>
{
    public async ValueTask<Result<string>> Handle(
        DeleteRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        Role? role = await manager.FindByIdAsync(command.RoleId, cancellationToken);
        if (role == null)
        {
            return Result<string>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    Messenger
                        .Create<Role>()
                        .Message(MessageType.Found)
                        .Negative()
                        .VietnameseTranslation(TranslatableMessage.VI_ROLE_NOT_FOUND)
                        .BuildMessage()
                )
            );
        }

        await manager.DeleteAsync(role, cancellationToken);
        return Result<string>.Success();
    }
}
