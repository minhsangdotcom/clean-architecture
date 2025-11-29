using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Validators;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Messages;
using Domain.Aggregates.Users;
using FluentValidation;

namespace Application.Features.Users.Commands.ResetPassword;

public class UpdateUserPasswordValidator(
    IHttpContextAccessorService httpContextAccessor,
    IMessageTranslatorService translator
) : FluentValidator<UpdateUserPassword>(httpContextAccessor, translator)
{
    protected sealed override void ApplyRules(IMessageTranslatorService translator)
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

    protected sealed override void ApplyRules(
        IHttpContextAccessorService contextAccessor,
        IMessageTranslatorService translator
    ) { }
}
