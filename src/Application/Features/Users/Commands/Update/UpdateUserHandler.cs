using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.Services.Storage;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Roles.Specifications;
using Domain.Aggregates.Users;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserHandler(
    IUserManager userManager,
    IEfUnitOfWork unitOfWork,
    IMediaStorageService<User> storageService,
    IMessageTranslatorService translator
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
                        translator.Translate(UserErrorMessages.UserNotFound)
                    )
                )
            );
        }
        UserUpdateData updateData = command.UpdateData;

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
            IList<string> roles = await unitOfWork
                .ReadOnlyRepository<Role>()
                .ListAsync(
                    new GetRoleNameByListRoleIdSpecification(updateData.Roles!),
                    cancellationToken
                );
            await userManager.ReplaceRolesAsync(user, roles, cancellationToken);

            // add permissions
            if (updateData.Permissions?.Count > 0)
            {
                List<Permission> permissions = await unitOfWork
                    .Repository<Permission>()
                    .ListAsync(
                        x => updateData.Permissions.Contains(x.Id),
                        cancellationToken: cancellationToken
                    );
                await userManager.ReplacePermissionsAsync(user, permissions, cancellationToken);
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
        var response = await userManager.FindByIdAsync(
            user.Id,
            cancellationToken: cancellationToken
        );
        return Result<UpdateUserResponse>.Success(response!.ToUpdateUserResponse());
    }
}
