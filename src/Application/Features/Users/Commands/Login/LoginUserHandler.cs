using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.Services.Token;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Messages;
using Application.SharedFeatures.Mapping.Users;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using DotNetCoreExtension.Extensions;
using Mediator;
using SharedKernel.Constants;
using Wangkanai.Detection.Services;

namespace Application.Features.Users.Commands.Login;

public class LoginUserHandler(
    IEfUnitOfWork unitOfWork,
    ITokenFactoryService tokenFactory,
    IDetectionService detectionService,
    ICurrentUser currentUser,
    IMessageTranslatorService translator
) : IRequestHandler<LoginUserCommand, Result<LoginUserResponse>>
{
    public async ValueTask<Result<LoginUserResponse>> Handle(
        LoginUserCommand command,
        CancellationToken cancellationToken
    )
    {
        User? user = await unitOfWork
            .ReadonlyRepository<User>()
            .FindByConditionAsync(
                new GetUserByIdentifierSpecification(command.Identifier!),
                cancellationToken
            );
        if (user == null)
        {
            string errorMessage = Messenger
                .Create<User>()
                .WithError(MessageErrorType.Found)
                .Negative()
                .GetFullMessage();
            return Result<LoginUserResponse>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    new(
                        UserErrorMessages.UserNotFound,
                        translator.Translate(UserErrorMessages.UserNotFound)
                    )
                )
            );
        }
        if (!Verify(command.Password, user.Password))
        {
            string errorMessage = Messenger
                .Create<User>()
                .Property(x => x.Password)
                .WithError(MessageErrorType.Correct)
                .Negative()
                .GetFullMessage();
            return Result<LoginUserResponse>.Failure(
                new BadRequestError(
                    "Error has occurred with password",
                    new(
                        UserErrorMessages.UserPasswordIncorrect,
                        translator.Translate(UserErrorMessages.UserPasswordIncorrect)
                    )
                )
            );
        }

        DateTime refreshExpireTime = tokenFactory.RefreshTokenExpiredTime;
        string familyId = StringExtension.GenerateRandomString(32);
        string userAgent = detectionService.UserAgent.ToString();

        UserRefreshToken userRefreshToken =
            new()
            {
                ExpiredTime = refreshExpireTime,
                UserId = user.Id,
                FamilyId = familyId,
                UserAgent = userAgent,
                ClientIp = currentUser.ClientIp,
            };

        DateTime accessTokenExpiredTime = tokenFactory.AccessTokenExpiredTime;
        string accessToken = tokenFactory.CreateToken(
            [new(ClaimTypes.Sub, user.Id.ToString())],
            accessTokenExpiredTime
        );

        string refreshToken = tokenFactory.CreateToken(
            [new(ClaimTypes.TokenFamilyId, familyId), new(ClaimTypes.Sub, user.Id.ToString())]
        );

        userRefreshToken.Token = refreshToken;

        await unitOfWork
            .Repository<UserRefreshToken>()
            .AddAsync(userRefreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LoginUserResponse>.Success(
            new()
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiredIn = (long)
                    Math.Ceiling((accessTokenExpiredTime - DateTime.UtcNow).TotalSeconds),
                TokenType = AuthenticationSchemeDefinition.Bearer,
                User = user.ToUserProjection(),
            }
        );
    }
}
