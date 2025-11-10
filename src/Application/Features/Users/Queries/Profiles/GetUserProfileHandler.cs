using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Mediator;
using SharedKernel.Common.Messages;

namespace Application.Features.Users.Queries.Profiles;

public class GetUserProfileHandler(IEfUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<GetUserProfileQuery, Result<GetUserProfileResponse>>
{
    public async ValueTask<Result<GetUserProfileResponse>> Handle(
        GetUserProfileQuery query,
        CancellationToken cancellationToken
    )
    {
        GetUserProfileResponse? user = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .FindByConditionAsync(
                new GetUserByIdSpecification(currentUser.Id!.Value),
                x => x.ToGetUserProfileResponse(),
                cancellationToken
            );

        if (user == null)
        {
            return Result<GetUserProfileResponse>.Failure(
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

        return Result<GetUserProfileResponse>.Success(user);
    }
}
