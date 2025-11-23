using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Messages;
using Domain.Aggregates.Roles;
using Mediator;
using Microsoft.Extensions.Localization;

namespace Application.Features.Roles.Commands.Delete;

public class DeleteRoleHandler(
    IRoleManager manager,
    IStringLocalizer<DeleteRoleHandler> stringLocalizer
) : IRequestHandler<DeleteRoleCommand, Result<string>>
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
            string errorMessage = Messenger
                .Create<Role>()
                .WithError(MessageErrorType.Found)
                .Negative()
                .GetFullMessage();
            return Result<string>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    new(errorMessage, stringLocalizer[errorMessage])
                )
            );
        }

        await manager.DeleteAsync(role, cancellationToken);
        return Result<string>.Success();
    }
}
