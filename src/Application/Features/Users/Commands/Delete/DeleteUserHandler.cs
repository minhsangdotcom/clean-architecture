using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Storage;
using Application.Contracts.ApiWrapper;
using Domain.Aggregates.Users;
using Mediator;
using SharedKernel.Common.Messages;

namespace Application.Features.Users.Commands.Delete;

public class DeleteUserHandler(
    IUserManager userManager,
    IMediaStorageService<User> mediaUpdateService
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
                    Messenger
                        .Create<User>()
                        .Message(MessageType.Found)
                        .Negative()
                        .VietnameseTranslation(TranslatableMessage.VI_USER_NOT_FOUND)
                        .BuildMessage()
                )
            );
        }
        string? avatar = user.Avatar;

        await userManager.DeleteAsync(user, cancellationToken);
        await mediaUpdateService.DeleteAsync(avatar);
        return Result<string>.Success();
    }
}
