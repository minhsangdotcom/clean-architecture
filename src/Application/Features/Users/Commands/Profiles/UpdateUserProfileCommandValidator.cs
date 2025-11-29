using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Validators;
using Application.Contracts.ApiWrapper;
using FluentValidation;

namespace Application.Features.Users.Commands.Profiles;

public class UpdateUserProfileCommandValidator(
    IEfUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IHttpContextAccessorService contextAccessorService,
    IMessageTranslatorService translator
) : FluentValidator<UpdateUserProfileCommand>(contextAccessorService, translator)
{
    protected sealed override void ApplyRules(IMessageTranslatorService translator)
    {
        Ulid id = currentUser.Id!.Value;
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

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserEmailRequired,
                translator.Translate(UserErrorMessages.UserEmailRequired)
            ))
            .Must(x => x!.IsValidEmail())
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserEmailInvalid,
                translator.Translate(UserErrorMessages.UserEmailInvalid)
            ))
            .UserEmailAvailable(unitOfWork, id)
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserEmailExistent,
                translator.Translate(UserErrorMessages.UserEmailExistent)
            ));

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserGenderNotInEnum,
                translator.Translate(UserErrorMessages.UserGenderNotInEnum)
            ));
    }

    protected sealed override void ApplyRules(
        IHttpContextAccessorService contextAccessor,
        IMessageTranslatorService translator
    ) { }
}
