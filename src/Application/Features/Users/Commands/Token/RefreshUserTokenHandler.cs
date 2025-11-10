using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Token;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Specifications;
using Mediator;
using SharedKernel.Common.Messages;
using SharedKernel.Constants;
using SharedKernel.Extensions;
using SharedKernel.Models;
using Wangkanai.Detection.Services;

namespace Application.Features.Users.Commands.Token;

public class RefreshUserTokenHandler(
    IEfUnitOfWork unitOfWork,
    ITokenFactoryService tokenFactory,
    IDetectionService detectionService,
    ICurrentUser currentUser
) : IRequestHandler<RefreshUserTokenCommand, Result<RefreshUserTokenResponse>>
{
    public async ValueTask<Result<RefreshUserTokenResponse>> Handle(
        RefreshUserTokenCommand command,
        CancellationToken cancellationToken
    )
    {
        bool isValid = ValidateRefreshToken(
            command.RefreshToken!,
            out DecodeTokenResponse? decodeToken
        );
        if (!isValid)
        {
            return Result<RefreshUserTokenResponse>.Failure(
                new BadRequestError(
                    "Error has occurred with the Refresh token",
                    Messenger
                        .Create<UserToken>(nameof(User))
                        .Property(x => x.RefreshToken!)
                        .Message(MessageType.Valid)
                        .Negative()
                        .BuildMessage()
                )
            );
        }

        IList<UserToken> refreshTokens = await unitOfWork
            .DynamicReadOnlyRepository<UserToken>()
            .ListAsync(
                new ListRefreshTokenByFamilyIdSpecification(
                    decodeToken!.FamilyId!,
                    Ulid.Parse(decodeToken.Sub!)
                ),
                new()
                {
                    Sort = $"{nameof(UserToken.CreatedAt)}{OrderTerm.DELIMITER}{OrderTerm.DESC}",
                },
                deep: 0,
                cancellationToken: cancellationToken
            );

        if (refreshTokens.Count <= 0)
        {
            return Result<RefreshUserTokenResponse>.Failure(
                new UnauthorizedError(
                    "Error has occurred with the Refresh token",
                    Messenger
                        .Create<UserToken>(nameof(User))
                        .Property(x => x.RefreshToken!)
                        .Negative()
                        .Message(MessageType.Identical)
                        .Object("TheCurrentOne")
                        .BuildMessage()
                )
            );
        }
        UserToken validRefreshToken = refreshTokens[0];

        // detect cheating with token, maybe which is stolen
        if (validRefreshToken.RefreshToken != command.RefreshToken)
        {
            // remove all the token by family token
            await unitOfWork.Repository<UserToken>().DeleteRangeAsync(refreshTokens);
            await unitOfWork.SaveAsync(cancellationToken);

            return Result<RefreshUserTokenResponse>.Failure(
                new UnauthorizedError(
                    "Error has occurred with the Refresh token",
                    Messenger
                        .Create<UserToken>(nameof(User))
                        .Property(x => x.RefreshToken!)
                        .Negative()
                        .Message(MessageType.Identical)
                        .Object("TheCurrentOne")
                        .BuildMessage()
                )
            );
        }

        if (validRefreshToken.User?.Status == UserStatus.Inactive)
        {
            return Result<RefreshUserTokenResponse>.Failure(
                new BadRequestError(
                    "Error has occurred with the current user",
                    Messenger.Create<User>().Message(MessageType.Active).Negative().BuildMessage()
                )
            );
        }

        var accessTokenExpiredTime = tokenFactory.AccessTokenExpiredTime;
        var accessToken = tokenFactory.CreateToken(
            [new(ClaimTypes.Sub, decodeToken.Sub!)],
            accessTokenExpiredTime
        );

        var refreshTokenExpiredTime = tokenFactory.RefreshTokenExpiredTime;
        string refreshToken = tokenFactory.CreateToken(
            [
                new(ClaimTypes.Sub, decodeToken.Sub!),
                new(ClaimTypes.TokenFamilyId, decodeToken.FamilyId!),
            ],
            refreshTokenExpiredTime
        );

        var userAgent = new
        {
            Agent = detectionService.UserAgent.ToString(),
            DeviceType = detectionService.Device.Type,
            Platform = detectionService.Platform.Name,
            Browser = detectionService.Browser.Name,
            Engine = detectionService.Engine.Name,
        };

        var userToken = new UserToken()
        {
            FamilyId = decodeToken.FamilyId,
            UserId = Ulid.Parse(decodeToken.Sub!),
            ExpiredTime = refreshTokenExpiredTime,
            RefreshToken = refreshToken,
            UserAgent = SerializerExtension.Serialize(userAgent).StringJson,
            ClientIp = currentUser.ClientIp,
        };

        await unitOfWork.Repository<UserToken>().AddAsync(userToken, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);

        return Result<RefreshUserTokenResponse>.Success(
            new() { Token = accessToken, RefreshToken = refreshToken }
        );
    }

    private bool ValidateRefreshToken(string token, out DecodeTokenResponse? decodeTokenResponse)
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
