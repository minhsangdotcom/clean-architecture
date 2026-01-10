using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Users.Commands.Profiles;

public class UpdateUserProfileCommandValidator(
    IEfUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IRequestContextProvider contextProvider,
    ITranslator<Messages> translator
) : FluentValidator<UpdateUserProfileCommand>(contextProvider, translator)
{
    protected sealed override void ApplyRules(ITranslator<Messages> translator)
    {
        Ulid id = currentUser.Id!.Value;
        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserLastNameRequired)
            .MaximumLength(256)
            .WithTranslatedError(translator, UserErrorMessages.UserLastNameTooLong);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserFirstNameRequired)
            .MaximumLength(256)
            .WithTranslatedError(translator, UserErrorMessages.UserFirstNameTooLong);

        RuleFor(x => x.PhoneNumber)
            .BeValidPhoneNumber()
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithTranslatedError(translator, UserErrorMessages.UserPhoneNumberInvalid);

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserEmailRequired)
            .BeValidEmail()
            .WithTranslatedError(translator, UserErrorMessages.UserEmailInvalid)
            .BeUniqueUserEmail(unitOfWork, id)
            .WithTranslatedError(translator, UserErrorMessages.UserEmailExistent);

        RuleFor(x => x.Gender)
            .IsInEnum()
            .When(x => x.Gender != null, ApplyConditionTo.CurrentValidator)
            .WithTranslatedError(translator, UserErrorMessages.UserGenderNotInEnum);
    }

    protected sealed override void ApplyRules(
        IRequestContextProvider contextProvider,
        ITranslator<Messages> translator
    ) { }
}
