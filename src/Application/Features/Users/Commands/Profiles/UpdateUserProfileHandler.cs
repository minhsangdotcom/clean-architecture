using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Storage;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Messages;
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
            string errorMessage = Messenger
                .Create<User>()
                .WithError(MessageErrorType.Found)
                .Negative()
                .GetFullMessage();
            return Result<UpdateUserProfileResponse>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    new(errorMessage, stringLocalizer[errorMessage])
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
