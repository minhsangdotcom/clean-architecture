using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Localization;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Domain.Aggregates.Users;
using Mediator;

namespace Application.Features.Users.Queries.Profiles;

public class GetUserProfileHandler(
    IUserManager userManager,
    ICurrentUser currentUser,
    IMessageTranslatorService translator
) : IRequestHandler<GetUserProfileQuery, Result<GetUserProfileResponse>>
{
    public async ValueTask<Result<GetUserProfileResponse>> Handle(
        GetUserProfileQuery query,
        CancellationToken cancellationToken
    )
    {
        User? user = await userManager.FindByIdAsync(
            currentUser.Id!.Value,
            cancellationToken: cancellationToken
        );

        if (user == null)
        {
            return Result<GetUserProfileResponse>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    new(
                        UserErrorMessages.UserNotFound,
                        translator.Translate(UserErrorMessages.UserNotFound)
                    )
                )
            );
        }

        return Result<GetUserProfileResponse>.Success(user.ToGetUserProfileResponse());
    }
}
