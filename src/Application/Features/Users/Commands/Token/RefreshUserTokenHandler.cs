using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Token;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Dtos.Responses;
using Application.Contracts.Messages;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Specifications;
using DotNetCoreExtension.Extensions;
using Mediator;
using Microsoft.Extensions.Localization;
using SharedKernel.Constants;
using Wangkanai.Detection.Services;

namespace Application.Features.Users.Commands.Token;

public class RefreshUserTokenHandler(
    IEfUnitOfWork unitOfWork,
    ITokenFactoryService tokenFactory,
    IDetectionService detectionService,
    ICurrentUser currentUser,
    IStringLocalizer<RefreshUserTokenHandler> stringLocalizer
) : IRequestHandler<RefreshUserTokenCommand, Result<RefreshUserTokenResponse>>
{
    public async ValueTask<Result<RefreshUserTokenResponse>> Handle(
        RefreshUserTokenCommand command,
        CancellationToken cancellationToken
    )
    {
        bool isValid = DecodeRefreshToken(
            command.RefreshToken!,
            out DecodeTokenResponse? decodeToken
        );
        if (!isValid)
        {
            return Result<RefreshUserTokenResponse>.Failure(
                new BadRequestError(
                    "Error has occurred with the Refresh token",
                    new(
                        UserErrorMessages.UserRefreshTokenInvalid,
                        stringLocalizer[UserErrorMessages.UserRefreshTokenInvalid]
                    )
                )
            );
        }

        IList<UserRefreshToken> refreshTokens = await unitOfWork
            .DynamicReadOnlyRepository<UserRefreshToken>()
            .ListAsync(
                new ListRefreshTokenByFamilyIdSpecification(
                    decodeToken!.FamilyId!,
                    Ulid.Parse(decodeToken.Sub!)
                ),
                queryParam: new()
                {
                    Sort =
                        $"{nameof(UserRefreshToken.CreatedAt)}{OrderTerm.DELIMITER}{OrderTerm.DESC}",
                },
                deep: 0,
                cancellationToken: cancellationToken
            );

        if (refreshTokens.Count <= 0)
        {
            string errorMessage = Messenger
                .Create<UserRefreshToken>(nameof(User))
                .Property(x => x.Token!)
                .Negative()
                .WithError(MessageErrorType.Identical)
                .ToObject("TheCurrentOne")
                .GetFullMessage();
            return Result<RefreshUserTokenResponse>.Failure(
                new UnauthorizedError(
                    "Error has occurred with the Refresh token",
                    new(
                        UserErrorMessages.UserRefreshTokenIdentical,
                        stringLocalizer[UserErrorMessages.UserRefreshTokenIdentical]
                    )
                )
            );
        }
        UserRefreshToken validRefreshToken = refreshTokens[0];

        // detect cheating with token, maybe which is stolen
        if (validRefreshToken.Token != command.RefreshToken)
        {
            // remove all the token by family token
            await unitOfWork.Repository<UserRefreshToken>().DeleteRangeAsync(refreshTokens);
            await unitOfWork.SaveAsync(cancellationToken);

            string errorMessage = Messenger
                .Create<UserRefreshToken>(nameof(User))
                .Property(x => x.Token!)
                .Negative()
                .WithError(MessageErrorType.Identical)
                .ToObject("TheCurrentOne")
                .GetFullMessage();
            return Result<RefreshUserTokenResponse>.Failure(
                new UnauthorizedError(
                    "Error has occurred with the Refresh token",
                    new(
                        UserErrorMessages.UserRefreshTokenIdentical,
                        stringLocalizer[UserErrorMessages.UserRefreshTokenIdentical]
                    )
                )
            );
        }

        if (validRefreshToken.ExpiredTime <= DateTimeOffset.UtcNow)
        {
            string errorMessage = Messenger
                .Create<UserRefreshToken>(nameof(User))
                .Property(x => x.Token!)
                .WithError(MessageErrorType.Expired)
                .GetFullMessage();
            return Result<RefreshUserTokenResponse>.Failure(
                new BadRequestError(
                    "Error has occurred with refresh token",
                    new(
                        UserErrorMessages.UserRefreshTokenExpired,
                        stringLocalizer[UserErrorMessages.UserRefreshTokenExpired]
                    )
                )
            );
        }

        if (validRefreshToken.User?.Status == UserStatus.Inactive)
        {
            string errorMessage = Messenger
                .Create<User>()
                .WithError(MessageErrorType.Active)
                .Negative()
                .GetFullMessage();
            return Result<RefreshUserTokenResponse>.Failure(
                new BadRequestError(
                    "Error has occurred with the current user",
                    new(
                        UserErrorMessages.UserInactiveForRefreshToken,
                        stringLocalizer[UserErrorMessages.UserInactiveForRefreshToken]
                    )
                )
            );
        }

        DateTime accessTokenExpiredTime = tokenFactory.AccessTokenExpiredTime;
        string accessToken = tokenFactory.CreateToken(
            [new(ClaimTypes.Sub, decodeToken.Sub!)],
            accessTokenExpiredTime
        );

        string refreshToken = tokenFactory.CreateToken(
            [
                new(ClaimTypes.Sub, decodeToken.Sub!),
                new(ClaimTypes.TokenFamilyId, decodeToken.FamilyId!),
            ]
        );

        var userAgent = new
        {
            Agent = detectionService.UserAgent.ToString(),
            DeviceType = detectionService.Device.Type,
            Platform = detectionService.Platform.Name,
            Browser = detectionService.Browser.Name,
            Engine = detectionService.Engine.Name,
        };

        UserRefreshToken userRefreshToken =
            new()
            {
                FamilyId = decodeToken.FamilyId,
                UserId = Ulid.Parse(decodeToken.Sub!),
                ExpiredTime = tokenFactory.RefreshTokenExpiredTime,
                Token = refreshToken,
                UserAgent = SerializerExtension.Serialize(userAgent).StringJson,
                ClientIp = currentUser.ClientIp,
            };

        await unitOfWork
            .Repository<UserRefreshToken>()
            .AddAsync(userRefreshToken, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);

        return Result<RefreshUserTokenResponse>.Success(
            new() { Token = accessToken, RefreshToken = refreshToken }
        );
    }

    private bool DecodeRefreshToken(string token, out DecodeTokenResponse? decodeTokenResponse)
    {
        try
        {
            decodeTokenResponse = tokenFactory.DecodeToken(token);
            return true;
        }
        catch (Exception)
        {
            decodeTokenResponse = null!;
            return false;
        }
    }
}
