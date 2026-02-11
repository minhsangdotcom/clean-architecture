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
    ITokenService tokenService,
    IDetectionService detectionService,
    ICurrentUser currentUser,
    ITranslator<Messages> translator
) : IRequestHandler<RefreshUserTokenCommand, Result<RefreshUserTokenResponse>>
{
    public async ValueTask<Result<RefreshUserTokenResponse>> Handle(
        RefreshUserTokenCommand command,
        CancellationToken cancellationToken
    )
    {
        DecodedToken? decodedToken = tokenService.Decode<DecodedToken>(command.RefreshToken!);
        if (
            decodedToken == null
            || string.IsNullOrWhiteSpace(decodedToken.FamilyId)
            || string.IsNullOrWhiteSpace(decodedToken.Sub)
        )
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
                    decodedToken.FamilyId,
                    Ulid.Parse(decodedToken.Sub)
                ),
                queryParam: new()
                {
                    Sort =
                        $"{nameof(UserRefreshToken.CreatedAt)}{OrderTerm.DELIMITER}{OrderTerm.DESC},{nameof(UserRefreshToken.Id)}{OrderTerm.DELIMITER}{OrderTerm.DESC}",
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

        DateTime accessTokenExpiredTime = tokenService.AccessTokenExpirationTime;
        string accessToken = tokenService.Generate(
            new Dictionary<string, object>() { { ClaimTypes.Sub, decodedToken.Sub } },
            accessTokenExpiredTime
        );

        string refreshToken = tokenService.Generate(
            new Dictionary<string, object>
            {
                { ClaimTypes.TokenFamilyId, decodedToken.FamilyId },
                { ClaimTypes.Sub, decodedToken.Sub },
                { "jti", Guid.NewGuid().ToString() },
            }
        );

        var userAgent = new
        {
            Agent = detectionService.UserAgent.ToString(),
            DeviceType = detectionService.Device.Type,
            Platform = detectionService.Platform.Name,
            Browser = detectionService.Browser.Name,
            Engine = detectionService.Engine.Name,
        };
        DateTimeOffset refreshTokenExpiredTime = tokenService.RefreshTokenExpirationTime;
        UserRefreshToken userRefreshToken = new()
        {
            FamilyId = decodedToken.FamilyId,
            UserId = Ulid.Parse(decodedToken.Sub),
            ExpiredTime = refreshTokenExpiredTime,
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
}
