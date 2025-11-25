using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Messages;
using Domain.Aggregates.Users;
using Mediator;
using Microsoft.Extensions.Localization;

namespace Application.Features.Users.Queries.Detail;

public class GetUserDetailHandler(
    IUserManager userManager,
    IStringLocalizer<GetUserDetailHandler> stringLocalizer
) : IRequestHandler<GetUserDetailQuery, Result<GetUserDetailResponse>>
{
    public async ValueTask<Result<GetUserDetailResponse>> Handle(
        GetUserDetailQuery query,
        CancellationToken cancellationToken
    )
    {
        User? user = await userManager.FindByIdAsync(
            Ulid.Parse(query.UserId),
            cancellationToken: cancellationToken
        );

        if (user == null)
        {
            return Result<GetUserDetailResponse>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    new(
                        UserErrorMessages.UserNotFound,
                        stringLocalizer[UserErrorMessages.UserNotFound]
                    )
                )
            );
        }

        return Result<GetUserDetailResponse>.Success(user.ToGetUserDetailResponse());
    }
}
