using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Storage;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Domain.Aggregates.Users;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace Application.Features.Users.Commands.Profiles;

public class UpdateUserProfileHandler(
    IUserManager userManager,
    IMediaStorageService<User> storageService,
    IStringLocalizer<UpdateUserProfileHandler> stringLocalizer,
    ICurrentUser currentUser
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
                    new(
                        UserErrorMessages.UserNotFound,
                        stringLocalizer[UserErrorMessages.UserNotFound]
                    )
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
            user.Id,
            cancellationToken: cancellationToken
        );

        return Result<UpdateUserProfileResponse>.Success(response!.ToUpdateUserProfileResponse());
    }
}
