using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Mediator;
using Microsoft.AspNetCore.Http;
using SharedKernel.Common.Messages;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserHandler(
    IEfUnitOfWork unitOfWork,
    IMediaUpdateService<User> mediaUpdateService
) : IRequestHandler<UpdateUserCommand, Result<UpdateUserResponse>>
{
    public async ValueTask<Result<UpdateUserResponse>> Handle(
        UpdateUserCommand command,
        CancellationToken cancellationToken
    )
    {
        User? user = await GetUserAsync(Ulid.Parse(command.UserId), cancellationToken);
        if (user == null)
        {
            return Result<UpdateUserResponse>.Failure(
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

        UserUpdateRequest updateData = command.UpdateData;

        IFormFile? avatar = updateData.Avatar;
        string? oldAvatar = user.Avatar;

        user.FromUpdateUser(updateData);

        string? key = mediaUpdateService.GetKey(avatar);
        user.ChangeAvatar(await mediaUpdateService.UploadAvatarAsync(avatar, key));

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await unitOfWork.Repository<User>().UpdateAsync(user);
            await unitOfWork.SaveAsync(cancellationToken);

            // update permissions and roles
            //
            await unitOfWork.CommitAsync(cancellationToken);

            await mediaUpdateService.DeleteAvatarAsync(oldAvatar);
            User? userResponse = await GetUserAsync(user.Id, cancellationToken);
            return Result<UpdateUserResponse>.Success(userResponse!.ToUpdateUserResponse());
        }
        catch (Exception)
        {
            await mediaUpdateService.DeleteAvatarAsync(user.Avatar);
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<User?> GetUserAsync(Ulid id, CancellationToken cancellationToken)
    {
        return await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .FindByConditionAsync(new GetUserByIdSpecification(id), cancellationToken);
    }
}
