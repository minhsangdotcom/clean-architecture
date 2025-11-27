using Application.Common.ErrorCodes;
using Application.Common.Extensions;
using Application.Common.Interfaces.Services.Localization;
using Application.Contracts.ApiWrapper;
using FluentValidation;

namespace Application.Features.Users.Commands.Profiles;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator(IMessageTranslatorService translator)
    {
        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserLastNameRequired,
                translator.Translate(UserErrorMessages.UserLastNameRequired)
            ))
            .MaximumLength(256)
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserLastNameTooLong,
                translator.Translate(UserErrorMessages.UserLastNameTooLong)
            ));

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserFirstNameRequired,
                translator.Translate(UserErrorMessages.UserFirstNameRequired)
            ))
            .MaximumLength(256)
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserFirstNameTooLong,
                translator.Translate(UserErrorMessages.UserFirstNameTooLong)
            ));

        RuleFor(x => x.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .Must(x => x!.IsValidPhoneNumber())
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserPhoneNumberInvalid,
                translator.Translate(UserErrorMessages.UserPhoneNumberInvalid)
            ));

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserGenderNotInEnum,
                translator.Translate(UserErrorMessages.UserGenderNotInEnum)
            ));
    }
}
