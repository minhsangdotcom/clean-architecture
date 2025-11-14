using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Token;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Mapping.Users;
using Contracts.ApiWrapper;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using DotNetCoreExtension.Extensions;
using Mediator;
using SharedKernel.Common.Messages;
using SharedKernel.Constants;
using Wangkanai.Detection.Services;

namespace Application.Features.Users.Commands.Login;

public class LoginUserHandler(
    IEfUnitOfWork unitOfWork,
    ITokenFactoryService tokenFactory,
    IDetectionService detectionService,
    ICurrentUser currentUser
) : IRequestHandler<LoginUserCommand, Result<LoginUserResponse>>
{
    public async ValueTask<Result<LoginUserResponse>> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken
    )
    {
        User? user = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .FindByConditionAsync(
                new GetUserByUsernameSpecification(request.Username!),
                cancellationToken
            );
        if (user == null)
        {
            return Result<LoginUserResponse>.Failure(
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
        if (!Verify(request.Password, user.Password))
        {
            return Result<LoginUserResponse>.Failure(
                new BadRequestError(
                    "Error has occurred with password",
                    Messenger
                        .Create<User>()
                        .Property(x => x.Password)
                        .Message(MessageType.Correct)
                        .Negative()
                        .BuildMessage()
                )
            );
        }

        DateTime refreshExpireTime = tokenFactory.RefreshTokenExpiredTime;
        string familyId = StringExtension.GenerateRandomString(32);

        var userAgent = detectionService.UserAgent.ToString();

        var UserRefreshToken = new UserRefreshToken()
        {
            ExpiredTime = refreshExpireTime,
            UserId = user.Id,
            FamilyId = familyId,
            UserAgent = userAgent,
            ClientIp = currentUser.ClientIp,
        };

        var accessTokenExpiredTime = tokenFactory.AccessTokenExpiredTime;
        string accessToken = tokenFactory.CreateToken(
            [new(ClaimTypes.Sub, user.Id.ToString())],
            accessTokenExpiredTime
        );

        string refreshToken = tokenFactory.CreateToken(
            [new(ClaimTypes.TokenFamilyId, familyId), new(ClaimTypes.Sub, user.Id.ToString())],
            refreshExpireTime
        );

        UserRefreshToken.RefreshToken = refreshToken;

        await unitOfWork.Repository<UserRefreshToken>().AddAsync(UserRefreshToken, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);

        return Result<LoginUserResponse>.Success(
            new()
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiredIn = (long)
                    Math.Ceiling((accessTokenExpiredTime - DateTime.UtcNow).TotalSeconds),
                TokenType = currentUser.AuthenticationScheme,
                User = user.ToUserProjection(),
            }
        );
    }
}
