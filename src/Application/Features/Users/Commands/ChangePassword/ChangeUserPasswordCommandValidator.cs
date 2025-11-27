using Application.Common.Extensions;
using Application.Common.Interfaces.Services.Localization;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Messages;
using Domain.Aggregates.Users;
using FluentValidation;

namespace Application.Features.Users.Commands.ChangePassword;

public class ChangeUserPasswordCommandValidator : AbstractValidator<ChangeUserPasswordCommand>
{
    public ChangeUserPasswordCommandValidator(IMessageTranslatorService translator)
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty()
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<ChangeUserPasswordCommand>(nameof(User))
                    .Property(x => x.OldPassword!)
                    .WithError(MessageErrorType.Required)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, translator.Translate(errorMessage));
            });

        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<ChangeUserPasswordCommand>(nameof(User))
                    .Property(x => x.NewPassword!)
                    .WithError(MessageErrorType.Required)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, translator.Translate(errorMessage));
            })
            .Must(x => x!.IsValidPassword())
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<ChangeUserPasswordCommand>(nameof(User))
                    .Property(x => x.NewPassword!)
                    .WithError(MessageErrorType.Strong)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, translator.Translate(errorMessage));
            });
    }
}
