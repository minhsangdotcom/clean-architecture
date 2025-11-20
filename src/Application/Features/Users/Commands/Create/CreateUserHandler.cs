using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Storage;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Mediator;

namespace Application.Features.Users.Commands.Create;

public class CreateUserHandler(
    IEfUnitOfWork unitOfWork,
    IMediaStorageService<User> storageService,
    IUserManager userManager
) : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    public async ValueTask<Result<CreateUserResponse>> Handle(
        CreateUserCommand command,
        CancellationToken cancellationToken
    )
    {
        User user = command.ToUser();

        // upload avatar
        string? key = storageService.GetKey(command.Avatar);
        user.ChangeAvatar(await storageService.UploadAsync(command.Avatar, key));

        string? userAvatar = null;
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            _ = await userManager.CreateAsync(user, command.Password!, cancellationToken);
            userAvatar = user.Avatar;

            // add roles
            List<string> roles = await unitOfWork
                .Repository<Role>()
                .ListAsync(x => command.Roles!.Contains(x.Id), x => x.Name, cancellationToken);
            await userManager.AddToRolesAsync(user, roles, cancellationToken);

            // add permissions
            if (command.Permissions?.Count != 0)
            {
                List<Permission> permissions = await unitOfWork
                    .Repository<Permission>()
                    .ListAsync(
                        x => command.Permissions!.Contains(x.Id),
                        cancellationToken: cancellationToken
                    );
                await userManager.AddPermissionsAsync(user, permissions, cancellationToken);
            }
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await storageService.DeleteAsync(userAvatar);
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        User? userResponse = await userManager.FindByIdAsync(
            user.Id,
            cancellationToken: cancellationToken
        );
        return Result<CreateUserResponse>.Success(userResponse!.ToCreateUserResponse());
    }
}
