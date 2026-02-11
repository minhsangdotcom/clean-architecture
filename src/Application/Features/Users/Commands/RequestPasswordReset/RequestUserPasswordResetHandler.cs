using Application.Common.ErrorCodes;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.Services.Mail;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Constants;
using Application.Contracts.Dtos.Models;
using Application.Contracts.Dtos.Requests;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Specifications;
using DotNetCoreExtension.Extensions;
using Mediator;
using Microsoft.Extensions.Options;

namespace Application.Features.Users.Commands.RequestPasswordReset;

public class RequestUserPasswordResetHandler(
    IEfUnitOfWork unitOfWork,
    IMailService mailService,
    IOptions<ForgotPasswordSettings> options,
    ITranslator<Messages> translator
) : IRequestHandler<RequestUserPasswordResetCommand, Result<string>>
{
    private readonly ForgotPasswordSettings forgotPasswordSettings = options.Value;

    public async ValueTask<Result<string>> Handle(
        RequestUserPasswordResetCommand command,
        CancellationToken cancellationToken
    )
    {
        User? user = await unitOfWork
            .ReadonlyRepository<User>()
            .FindByConditionAsync(
                new GetUserByEmailIncludePasswordResetRequestSpecification(command.Email!),
                cancellationToken
            );

        if (user == null)
        {
            return Result<string>.Failure(
                new NotFoundError(
                    TitleMessage.RESOURCE_NOT_FOUND,
                    new(
                        UserErrorMessages.UserNotFound,
                        translator.Translate(UserErrorMessages.UserNotFound)
                    )
                )
            );
        }

        if (user.Status == UserStatus.Inactive)
        {
            return Result<string>.Failure(
                new BadRequestError(
                    "Error has occurred with the current user",
                    new(
                        UserErrorMessages.UserInactive,
                        translator.Translate(UserErrorMessages.UserInactive)
                    )
                )
            );
        }

        string token = StringExtension.GenerateRandomString(40);
        DateTimeOffset expiredTime = DateTimeOffset.UtcNow.AddHours(
            forgotPasswordSettings.ExpiredTimeInHour
        );

        UserPasswordReset userPasswordReset = new()
        {
            Token = token,
            UserId = user.Id,
            Expiry = expiredTime,
        };

        await unitOfWork
            .Repository<UserPasswordReset>()
            .DeleteRangeAsync(user.PasswordResetRequests);
        await unitOfWork
            .Repository<UserPasswordReset>()
            .AddAsync(userPasswordReset, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        UriBuilder linkBuilder = new(forgotPasswordSettings.Uri)
        {
            Query = $"token={Uri.EscapeDataString(userPasswordReset.Token)}&email={user.Email}",
        };

        MailTemplateData mail = new()
        {
            DisplayName = "The Template password Reset",
            Subject = "Reset password",
            To = [user.Email],
            Template = new(
                forgotPasswordSettings.TemplateName,
                new ResetPasswordModel()
                {
                    ResetLink = linkBuilder.ToString(),
                    ExpiredTimeInHour = forgotPasswordSettings.ExpiredTimeInHour,
                    UserEmail = user.Email,
                    SupportEmail = "minhsang.work25@gmail.com",
                    UserName = user.Username,
                    Year = DateTimeOffset.UtcNow.Year,
                }
            ),
        };

        await mailService.SendWithTemplateAsync(mail);
        return Result<string>.Success();
    }
}
