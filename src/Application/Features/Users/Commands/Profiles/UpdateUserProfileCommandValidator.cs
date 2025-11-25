using Application.Common.ErrorCodes;
using Application.Common.Extensions;
using Application.Contracts.ApiWrapper;
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
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserLastNameRequired,
                stringLocalizer[UserErrorMessages.UserLastNameRequired]
            ))
            .MaximumLength(256)
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserLastNameTooLong,
                stringLocalizer[UserErrorMessages.UserLastNameTooLong]
            ));

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserFirstNameRequired,
                stringLocalizer[UserErrorMessages.UserFirstNameRequired]
            ))
            .MaximumLength(256)
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserFirstNameTooLong,
                stringLocalizer[UserErrorMessages.UserFirstNameTooLong]
            ));

        RuleFor(x => x.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .Must(x => x!.IsValidPhoneNumber())
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserPhoneNumberInvalid,
                stringLocalizer[UserErrorMessages.UserPhoneNumberInvalid]
            ));

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserGenderNotInEnum,
                stringLocalizer[UserErrorMessages.UserGenderNotInEnum]
            ));
    }
}
