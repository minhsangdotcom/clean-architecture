using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Mediator;
using SharedKernel.Common.Messages;

namespace Application.Features.Users.Queries.Detail;

public class GetUserDetailHandler(IEfUnitOfWork unitOfWork)
    : IRequestHandler<GetUserDetailQuery, Result<GetUserDetailResponse>>
{
    public async ValueTask<Result<GetUserDetailResponse>> Handle(
        GetUserDetailQuery query,
        CancellationToken cancellationToken
    )
    {
        GetUserDetailResponse? user = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .FindByConditionAsync(
                new GetUserByIdSpecification(query.UserId),
                x => x.ToGetUserDetailResponse(),
                cancellationToken
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

        return Result<GetUserDetailResponse>.Success(user);
    }
}
