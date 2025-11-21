using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Contracts.ApiWrapper;
using Domain.Aggregates.Users;
using Mediator;
using SharedKernel.Common.Messages;

namespace Application.Features.Users.Queries.Detail;

public class GetUserDetailHandler(IUserManager userManager)
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
                    Messenger
                        .Create<User>()
                        .Message(MessageType.Found)
                        .Negative()
                        .VietnameseTranslation(TranslatableMessage.VI_USER_NOT_FOUND)
                        .Build()
                )
            );
        }

        return Result<GetUserDetailResponse>.Success(user.ToGetUserDetailResponse());
    }
}
