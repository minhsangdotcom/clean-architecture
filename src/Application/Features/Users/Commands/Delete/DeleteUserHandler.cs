using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Storage;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Messages;
using Domain.Aggregates.Users;
using Mediator;
using Microsoft.Extensions.Localization;

namespace Application.Features.Users.Commands.Delete;

public class DeleteUserHandler(
    IUserManager userManager,
    IMediaStorageService<User> mediaUpdateService,
    IStringLocalizer<DeleteUserHandler> stringLocalizer
) : IRequestHandler<DeleteUserCommand, Result<string>>
{
    public async ValueTask<Result<string>> Handle(
        DeleteUserCommand command,
        CancellationToken cancellationToken
    )
    {
        User? user = await userManager.FindByIdAsync(
            Ulid.Parse(command.UserId),
            false,
            cancellationToken
        );
        if (user == null)
        {
            return Result<string>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    new(
                        UserErrorMessages.UserNotFound,
                        stringLocalizer[UserErrorMessages.UserNotFound]
                    )
                )
            );
        }
        string? avatar = user.Avatar;

        await userManager.DeleteAsync(user, cancellationToken);
        await mediaUpdateService.DeleteAsync(avatar);
        return Result<string>.Success();
    }
}
