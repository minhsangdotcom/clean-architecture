using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Identity;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Messages;
using Domain.Aggregates.Users;
using Mediator;
using Microsoft.Extensions.Localization;

namespace Application.Features.Users.Queries.Profiles;

public class GetUserProfileHandler(
    IUserManager userManager,
    ICurrentUser currentUser,
    IStringLocalizer<GetUserProfileHandler> stringLocalizer
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
            string errorMessage = Messenger
                .Create<User>()
                .WithError(MessageErrorType.Found)
                .Negative()
                .GetFullMessage();
            return Result<GetUserProfileResponse>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    new(errorMessage, stringLocalizer[errorMessage])
                )
            );
        }

        return Result<GetUserProfileResponse>.Success(user.ToGetUserProfileResponse());
    }
}
