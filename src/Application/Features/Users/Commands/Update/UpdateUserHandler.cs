using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Regions;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Specifications;
using Mediator;
using Microsoft.AspNetCore.Http;
using SharedKernel.Common.Messages;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserHandler(
    IEfUnitOfWork unitOfWork,
    IMediaUpdateService<User> mediaUpdateService,
    IUserManagerService userManagerService
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

        Province? province = await unitOfWork
            .Repository<Province>()
            .FindByIdAsync(updateData.ProvinceId, cancellationToken);
        if (province == null)
        {
            return Result<UpdateUserResponse>.Failure<NotFoundError>(
                new(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    Messenger
                        .Create<User>()
                        .Property(nameof(UserUpdateRequest.ProvinceId))
                        .Message(MessageType.Existence)
                        .Negative()
                        .Build()
                )
            );
        }

        District? district = await unitOfWork
            .Repository<District>()
            .FindByIdAsync(updateData.DistrictId, cancellationToken);
        if (district == null)
        {
            return Result<UpdateUserResponse>.Failure<NotFoundError>(
                new(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    Messenger
                        .Create<User>()
                        .Property(nameof(updateData.DistrictId))
                        .Message(MessageType.Existence)
                        .Negative()
                        .Build()
                )
            );
        }

        Commune? commune = null;
        if (updateData.CommuneId.HasValue)
        {
            commune = await unitOfWork
                .Repository<Commune>()
                .FindByIdAsync(updateData.CommuneId.Value, cancellationToken);

            if (commune == null)
            {
                return Result<UpdateUserResponse>.Failure<NotFoundError>(
                    new(
                        TitleMessage.RESOURCE_NOT_FOUND,
                        Messenger
                            .Create<User>()
                            .Property(nameof(UserUpdateRequest.CommuneId))
                            .Message(MessageType.Existence)
                            .Negative()
                            .Build()
                    )
                );
            }
        }

        //* replace address
        user.UpdateAddress(
            new(
                province!.FullName,
                province.Id,
                district!.FullName,
                district.Id,
                commune?.FullName,
                commune?.Id,
                command.UpdateData.Street!
            )
        );

        string? key = mediaUpdateService.GetKey(avatar);
        user.Avatar = await mediaUpdateService.UploadAvatarAsync(avatar, key);

        //* trigger event to update default claims -  that's information of user
        user.UpdateDefaultUserClaims();

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            await unitOfWork.Repository<User>().UpdateAsync(user);
            await unitOfWork.SaveAsync(cancellationToken);

            //* update custom claims of user like permissions ...
            List<UserClaim> customUserClaims =
                updateData.UserClaims?.ToListUserClaim(UserClaimType.Custom, user.Id) ?? [];
            await userManagerService.UpdateAsync(user, updateData.Roles!, customUserClaims);

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
