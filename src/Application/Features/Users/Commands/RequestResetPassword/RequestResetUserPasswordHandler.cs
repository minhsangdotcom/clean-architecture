using Application.Common.Constants;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Mail;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Dtos.Models;
using Contracts.Dtos.Requests;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using DotNetCoreExtension.Extensions;
using Mediator;
using Microsoft.Extensions.Configuration;
using SharedKernel.Common.Messages;

namespace Application.Features.Users.Commands.RequestResetPassword;

public class RequestResetUserPasswordHandler(
    IEfUnitOfWork unitOfWork,
    IConfiguration configuration,
    IMailService mailService
) : IRequestHandler<RequestResetUserPasswordCommand, Result<string>>
{
    public async ValueTask<Result<string>> Handle(
        RequestResetUserPasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        User? user = await unitOfWork
            .DynamicReadOnlyRepository<User>()
            .FindByConditionAsync(
                new GetUserByEmailSpecification(command.Email),
                cancellationToken
            );

        if (user == null)
        {
            return Result<string>.Failure(
                new NotFoundError(
                    "the TitleMessage.RESOURCE_NOT_FOUND",
                    Messenger
                        .Create<User>()
                        .Message(MessageType.Found)
                        .Negative()
                        .VietnameseTranslation(TranslatableMessage.VI_USER_NOT_FOUND)
                        .Build()
                )
            );
        }

        string token = StringExtension.GenerateRandomString(40);
        DateTimeOffset expiredTime = DateTimeOffset.UtcNow.AddHours(
            configuration.GetValue<int>("ForgotPasswordExpiredTimeInHour")
        );
        UserPasswordReset UserPasswordReset =
            new()
            {
                Token = token,
                UserId = user.Id,
                Expiry = expiredTime,
            };

        await unitOfWork
            .Repository<UserPasswordReset>()
            .DeleteRangeAsync(user.PasswordResetRequests!);
        await unitOfWork
            .Repository<UserPasswordReset>()
            .AddAsync(UserPasswordReset, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);

        string domain = configuration.GetValue<string>("ForgotPasswordUrl")!;
        var link = new UriBuilder(domain) { Query = $"token={token}&id={user.Id}" };
        string expiry = expiredTime.ToLocalTime().ToString("dd/MM/yyyy hh:mm:ss");

        _ = await mailService.SendWithTemplateAsync(
            new MailTemplateData()
            {
                DisplayName = "The Template Reset password",
                Subject = "Reset password",
                To = [user.Email],
                Template = new(
                    "ForgotPassword",
                    new ResetPasswordModel() { ResetLink = link.ToString(), Expiry = expiry }
                ),
            }
        );
        return Result<string>.Success();
    }
}
