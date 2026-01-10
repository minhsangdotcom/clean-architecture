using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.Services.Token;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Dtos.Responses;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Specifications;
using DotNetCoreExtension.Extensions;
using Mediator;
using SharedKernel.Constants;
using Wangkanai.Detection.Services;

namespace Application.Features.Users.Commands.Token;

public class RefreshUserTokenHandler(
    IEfUnitOfWork unitOfWork,
    ITokenFactoryService tokenFactory,
    IDetectionService detectionService,
    ICurrentUser currentUser,
    IMessageTranslator translator
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
                    TitleMessage.REFRESH_TOKEN_ERROR,
                    new(
                        UserErrorMessages.UserRefreshTokenInvalid,
                        translator.Translate(UserErrorMessages.UserRefreshTokenInvalid)
                    )
                )
            );
        }

        IList<UserRefreshToken> refreshTokens = await unitOfWork
            .ReadonlyRepository<UserRefreshToken>()
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
            return Result<RefreshUserTokenResponse>.Failure(
                new UnauthorizedError(
                    TitleMessage.REFRESH_TOKEN_ERROR,
                    new(
                        UserErrorMessages.UserRefreshTokenNotExistents,
                        translator.Translate(UserErrorMessages.UserRefreshTokenNotExistents)
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
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<RefreshUserTokenResponse>.Failure(
                new UnauthorizedError(
                    TitleMessage.REFRESH_TOKEN_ERROR,
                    new(
                        UserErrorMessages.UserRefreshTokenNotIdentical,
                        translator.Translate(UserErrorMessages.UserRefreshTokenNotIdentical)
                    )
                )
            );
        }

        if (validRefreshToken.ExpiredTime <= DateTimeOffset.UtcNow)
        {
            return Result<RefreshUserTokenResponse>.Failure(
                new UnauthorizedError(
                    "Error has occurred with refresh token",
                    new(
                        UserErrorMessages.UserRefreshTokenExpired,
                        translator.Translate(UserErrorMessages.UserRefreshTokenExpired)
                    )
                )
            );
        }

        if (validRefreshToken.User?.Status == UserStatus.Inactive)
        {
            return Result<RefreshUserTokenResponse>.Failure(
                new UnauthorizedError(
                    "Error has occurred with the current user",
                    new(
                        UserErrorMessages.UserInactive,
                        translator.Translate(UserErrorMessages.UserInactive)
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
        await unitOfWork.SaveChangesAsync(cancellationToken);

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
