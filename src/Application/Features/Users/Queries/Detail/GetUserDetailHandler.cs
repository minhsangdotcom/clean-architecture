using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.Services.Localization;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Domain.Aggregates.Users;
using Mediator;

namespace Application.Features.Users.Queries.Detail;

public class GetUserDetailHandler(IUserManager userManager, IMessageTranslator translator)
    : IRequestHandler<GetUserDetailQuery, Result<GetUserDetailResponse>>
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
                        translator.Translate(UserErrorMessages.UserNotFound)
                    )
                )
            );
        }

        return Result<GetUserDetailResponse>.Success(user.ToGetUserDetailResponse());
    }
}
