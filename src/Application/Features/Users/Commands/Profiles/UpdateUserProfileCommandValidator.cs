using Application.Common.Extensions;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Messages;
using Domain.Aggregates.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Application.Features.Users.Commands.Profiles;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator(
        IStringLocalizer<UpdateUserProfileCommandValidator> stringLocalizer
    )
    {
        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.LastName)
                    .WithError(MessageErrorType.Required)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            })
            .MaximumLength(256)
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.LastName)
                    .WithError(MessageErrorType.TooLong)
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            });

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.FirstName)
                    .WithError(MessageErrorType.Required)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            })
            .MaximumLength(256)
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.FirstName)
                    .WithError(MessageErrorType.TooLong)
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            });

        RuleFor(x => x.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .Must(x => x!.IsValidPhoneNumber())
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.PhoneNumber!)
                    .WithError(MessageErrorType.Valid)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            });

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.Gender!)
                    .Negative()
                    .WithError(MessageErrorType.AmongTheAllowedOptions)
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            });
    }
}
