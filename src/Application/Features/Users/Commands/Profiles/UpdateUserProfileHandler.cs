using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Contracts.ApiWrapper;
using Domain.Aggregates.Regions;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Mediator;
using Microsoft.AspNetCore.Http;
using SharedKernel.Common.Messages;

namespace Application.Features.Users.Commands.Profiles;

public class UpdateUserProfileHandler(
    IEfUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IMediaUpdateService<User> avatarUpdate
) : IRequestHandler<UpdateUserProfileCommand, Result<UpdateUserProfileResponse>>
{
    public async ValueTask<Result<UpdateUserProfileResponse>> Handle(
        UpdateUserProfileCommand command,
        CancellationToken cancellationToken
    )
    {
        User? user = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .FindByConditionAsync(
                new GetUserByIdWithoutIncludeSpecification(currentUser.Id ?? Ulid.Empty),
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

        user.MapFromUpdateUserProfileCommand(command);

        Province? province = await unitOfWork
            .Repository<Province>()
            .FindByIdAsync(command.ProvinceId, cancellationToken);
        if (province == null)
        {
            return Result<UpdateUserProfileResponse>.Failure<NotFoundError>(
                new(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    Messenger
                        .Create<User>()
                        .Property(nameof(UpdateUserProfileCommand.ProvinceId))
                        .Message(MessageType.Existence)
                        .Negative()
                        .Build()
                )
            );
        }

        District? district = await unitOfWork
            .Repository<District>()
            .FindByIdAsync(command.DistrictId, cancellationToken);
        if (district == null)
        {
            return Result<UpdateUserProfileResponse>.Failure<NotFoundError>(
                new(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    Messenger
                        .Create<User>()
                        .Property(nameof(UpdateUserProfileCommand.DistrictId))
                        .Message(MessageType.Existence)
                        .Negative()
                        .Build()
                )
            );
        }

        Commune? commune = null;
        if (command.CommuneId.HasValue)
        {
            commune = await unitOfWork
                .Repository<Commune>()
                .FindByIdAsync(command.CommuneId.Value, cancellationToken);

            if (commune == null)
            {
                return Result<UpdateUserProfileResponse>.Failure<NotFoundError>(
                    new(
                        TitleMessage.RESOURCE_NOT_FOUND,
                        Messenger
                            .Create<User>()
                            .Property(nameof(UpdateUserProfileCommand.CommuneId))
                            .Message(MessageType.Existence)
                            .Negative()
                            .Build()
                    )
                );
            }
        }
        user.UpdateAddress(
            new(
                province!.FullName,
                province.Id,
                district!.FullName,
                district.Id,
                commune?.FullName,
                commune?.Id,
                command.Street!
            )
        );

        string? key = avatarUpdate.GetKey(avatar);
        user.Avatar = await avatarUpdate.UploadAvatarAsync(avatar, key);

        try
        {
            await unitOfWork.Repository<User>().UpdateAsync(user);
            await unitOfWork.SaveAsync(cancellationToken);
            await avatarUpdate.DeleteAvatarAsync(oldAvatar);
        }
        catch (Exception)
        {
            await avatarUpdate.DeleteAvatarAsync(user.Avatar);
            throw;
        }

        UpdateUserProfileResponse? response = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .FindByConditionAsync(
                new GetUserByIdSpecification(user.Id),
                x => x.ToUpdateUserProfileResponse(),
                cancellationToken
            );

        return Result<UpdateUserProfileResponse>.Success(response!);
    }
}
