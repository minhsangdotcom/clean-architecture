using Application.Common.Extensions;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Messages;
using Domain.Aggregates.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Application.Features.Users.Commands.ChangePassword;

public class ChangeUserPasswordCommandValidator : AbstractValidator<ChangeUserPasswordCommand>
{
    public ChangeUserPasswordCommandValidator(
        IStringLocalizer<ChangeUserPasswordCommandValidator> stringLocalizer
    )
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

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
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

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
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

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            });
    }
}
