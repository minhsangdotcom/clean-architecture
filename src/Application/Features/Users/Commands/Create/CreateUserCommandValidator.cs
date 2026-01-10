using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services.Accessors;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Validators;
using Application.SharedFeatures.Validators.Users;
using Domain.Aggregates.Users;
using FluentValidation;

namespace Application.Features.Users.Commands.Create;

public class CreateUserCommandValidator(
    IEfUnitOfWork unitOfWork,
    ITranslator<Messages> translator,
    IRequestContextProvider contextProvider
) : FluentValidator<CreateUserCommand>(contextProvider, translator)
{
    protected sealed override void ApplyRules(
        IRequestContextProvider contextProvider,
        ITranslator<Messages> translator
    )
    {
        Include(new UserValidator(unitOfWork, contextProvider, translator));

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserUsernameRequired)
            .BeValidUsername()
            .WithTranslatedError(translator, UserErrorMessages.UserUsernameInvalid)
            .MustAsync((username, ct) => IsUsernameAvailableAsync(username!, cancellationToken: ct))
            .WithTranslatedError(translator, UserErrorMessages.UserUsernameExistent);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserPasswordRequired)
            .BeValidPassword()
            .WithTranslatedError(translator, UserErrorMessages.UserPasswordWeak);

        RuleFor(x => x.Gender)
            .IsInEnum()
            .When(x => x.Gender != null, ApplyConditionTo.CurrentValidator)
            .WithTranslatedError(translator, UserErrorMessages.UserGenderNotInEnum);
    }

    protected sealed override void ApplyRules(ITranslator<Messages> translator) { }

    private async Task<bool> IsUsernameAvailableAsync(
        string username,
        Ulid? excludeId = null,
        CancellationToken cancellationToken = default
    ) =>
        !await unitOfWork
            .Repository<User>()
            .AnyAsync(
                x => x.Username == username && (!excludeId.HasValue || x.Id != excludeId),
                cancellationToken
            );
}
