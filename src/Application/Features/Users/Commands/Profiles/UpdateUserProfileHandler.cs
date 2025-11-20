using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Storage;
using Contracts.ApiWrapper;
using Domain.Aggregates.Users;
using Mediator;
using Microsoft.AspNetCore.Http;
using SharedKernel.Common.Messages;

namespace Application.Features.Users.Commands.Profiles;

public class UpdateUserProfileHandler(
    ICurrentUser currentUser,
    IMediaStorageService<User> storageService,
    IUserManager userManager
) : IRequestHandler<UpdateUserProfileCommand, Result<UpdateUserProfileResponse>>
{
    public async ValueTask<Result<UpdateUserProfileResponse>> Handle(
        UpdateUserProfileCommand command,
        CancellationToken cancellationToken
    )
    {
        User? user = await userManager.FindByIdAsync(
            currentUser.Id!.Value,
            false,
            cancellationToken
        );

        if (user == null)
        {
            return Result<UpdateUserProfileResponse>.Failure(
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

        IFormFile? avatar = command.Avatar;
        string? oldAvatar = user.Avatar;

        user.MapFromCommand(command);

        string? key = storageService.GetKey(avatar);
        user.ChangeAvatar(await storageService.UploadAsync(avatar, key));

        try
        {
            await userManager.UpdateAsync(user, cancellationToken);
            await storageService.DeleteAsync(oldAvatar);
        }
        catch (Exception)
        {
            await storageService.DeleteAsync(user.Avatar);
            throw;
        }

        User? response = await userManager.FindByIdAsync(
            currentUser.Id!.Value,
            cancellationToken: cancellationToken
        );

        return Result<UpdateUserProfileResponse>.Success(response!.ToUpdateUserProfileResponse());
    }
}
