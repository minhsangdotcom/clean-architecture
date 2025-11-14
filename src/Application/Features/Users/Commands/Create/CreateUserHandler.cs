using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Mediator;

namespace Application.Features.Users.Commands.Create;

public class CreateUserHandler(
    IEfUnitOfWork unitOfWork,
    IMediaUpdateService<User> mediaUpdateService
) : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    public async ValueTask<Result<CreateUserResponse>> Handle(
        CreateUserCommand command,
        CancellationToken cancellationToken
    )
    {
        User mappingUser = command.ToUser();

        //* adding user avatar
        string? key = mediaUpdateService.GetKey(command.Avatar);
        mappingUser.ChangeAvatar(await mediaUpdateService.UploadAvatarAsync(command.Avatar, key));

        string? userAvatar = null;
        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            User user = await unitOfWork
                .Repository<User>()
                .AddAsync(mappingUser, cancellationToken);
            userAvatar = user.Avatar;

            // add roles and permissions
            //    ????
            //
            CreateUserResponse? response = await unitOfWork
                .DynamicReadOnlyRepository<User>()
                .FindByConditionAsync(
                    new GetUserByIdSpecification(user.Id),
                    x => x.ToCreateUserResponse(),
                    cancellationToken
                );
            return Result<CreateUserResponse>.Success(response!);
        }
        catch (Exception)
        {
            await mediaUpdateService.DeleteAvatarAsync(userAvatar);
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
