using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Token;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Specifications;
using DotNetCoreExtension.Extensions;
using Mediator;
using SharedKernel.Common.Messages;
using SharedKernel.Constants;
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
        bool isValid = DecodeRefreshToken(
            command.RefreshToken!,
            out DecodeTokenResponse? decodeToken
        );
        if (!isValid)
        {
            return Result<RefreshUserTokenResponse>.Failure(
                new BadRequestError(
                    "Error has occurred with the Refresh token",
                    Messenger
                        .Create<UserRefreshToken>(nameof(User))
                        .Property(x => x.Token!)
                        .Message(MessageType.Valid)
                        .Negative()
                        .BuildMessage()
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
                new()
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
                    "Error has occurred with the Refresh token",
                    Messenger
                        .Create<UserRefreshToken>(nameof(User))
                        .Property(x => x.Token!)
                        .Negative()
                        .Message(MessageType.Identical)
                        .Object("TheCurrentOne")
                        .BuildMessage()
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

            return Result<RefreshUserTokenResponse>.Failure(
                new UnauthorizedError(
                    "Error has occurred with the Refresh token",
                    Messenger
                        .Create<UserRefreshToken>(nameof(User))
                        .Property(x => x.Token!)
                        .Negative()
                        .Message(MessageType.Identical)
                        .Object("TheCurrentOne")
                        .BuildMessage()
                )
            );
        }

        if (validRefreshToken.ExpiredTime <= DateTimeOffset.UtcNow)
        {
            return Result<RefreshUserTokenResponse>.Failure(
                new BadRequestError(
                    "Error has occurred with refresh token",
                    Messenger
                        .Create<UserRefreshToken>(nameof(User))
                        .Property(x => x.Token!)
                        .Message(MessageType.Expired)
                        .Build()
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
