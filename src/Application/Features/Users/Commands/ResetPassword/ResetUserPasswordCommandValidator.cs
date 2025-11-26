using Application.Common.Extensions;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Localization;
using Application.Contracts.Messages;
using Domain.Aggregates.Users;
using FluentValidation;

namespace Application.Features.Users.Commands.ResetPassword;

public class UpdateUserPasswordValidator : AbstractValidator<UpdateUserPassword>
{
    public UpdateUserPasswordValidator(IMessageTranslatorService translator)
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<UserPasswordReset>()
                    .Property(x => x.Token)
                    .WithError(MessageErrorType.Required)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, translator.Translate(errorMessage));
            });

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.Password!)
                    .WithError(MessageErrorType.Required)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, translator.Translate(errorMessage));
            })
            .Must(x => x!.IsValidPassword())
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>(nameof(User))
                    .Property(x => x.Password)
                    .WithError(MessageErrorType.Strong)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, translator.Translate(errorMessage));
            });
    }
}
