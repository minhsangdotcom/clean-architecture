using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Storage;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserHandler(
    IUserManager userManager,
    IEfUnitOfWork unitOfWork,
    IMediaStorageService<User> storageService,
    IStringLocalizer<UpdateUserHandler> stringLocalizer
) : IRequestHandler<UpdateUserCommand, Result<UpdateUserResponse>>
{
    public async ValueTask<Result<UpdateUserResponse>> Handle(
        UpdateUserCommand command,
        CancellationToken cancellationToken
    )
    {
        User? user = await userManager.FindByIdAsync(
            Ulid.Parse(command.UserId),
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            return Result<UpdateUserResponse>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    new(
                        UserErrorMessages.UserNotFound,
                        stringLocalizer[UserErrorMessages.UserNotFound]
                    )
                )
            );
        }
        UserUpdateRequest updateData = command.UpdateData;

        IFormFile? avatar = updateData.Avatar;
        string? oldAvatar = user.Avatar;

        user.FromUpdateUser(updateData);
        string? key = storageService.GetKey(avatar);
        user.ChangeAvatar(await storageService.UploadAsync(avatar, key));

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await userManager.UpdateAsync(user, cancellationToken);

            // add roles
            List<string> roles = await unitOfWork
                .Repository<Role>()
                .ListAsync(x => updateData.Roles!.Contains(x.Id), x => x.Name, cancellationToken);
            await userManager.AddToRolesAsync(user, roles, cancellationToken);

            // add permissions
            if (updateData.Permissions?.Count > 0)
            {
                List<Permission> permissions = await unitOfWork
                    .Repository<Permission>()
                    .ListAsync(
                        x => updateData.Permissions.Contains(x.Id),
                        cancellationToken: cancellationToken
                    );
                await userManager.AddPermissionsAsync(user, permissions, cancellationToken);
            }
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            // rollback new avatar
            await storageService.DeleteAsync(user.Avatar);
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
        // delete old avatar after updating Successfully
        await storageService.DeleteAsync(oldAvatar);
        return Result<UpdateUserResponse>.Success(user.ToUpdateUserResponse());
    }
}
