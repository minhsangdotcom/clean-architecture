using Application.Common.ErrorCodes;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Localization;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.Validators;
using Application.SharedFeatures.Validators.Users;
using Domain.Aggregates.Users;
using FluentValidation;

namespace Application.Features.Users.Commands.Create;

public class CreateUserCommandValidator(
    IEfUnitOfWork unitOfWork,
    IMessageTranslatorService translator,
    IHttpContextAccessorService contextAccessor
) : FluentValidator<CreateUserCommand>(contextAccessor, translator)
{
    protected sealed override void ApplyRules(
        IHttpContextAccessorService contextAccessor,
        IMessageTranslatorService translator
    )
    {
        Include(new UserValidator(unitOfWork, contextAccessor, translator));

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserUsernameRequired)
            .Must((_, x) => x!.IsValidUsername())
            .WithTranslatedError(translator, UserErrorMessages.UserUsernameInvalid)
            .MustAsync((username, ct) => IsUsernameAvailableAsync(username!, cancellationToken: ct))
            .WithTranslatedError(translator, UserErrorMessages.UserUsernameExistent);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithTranslatedError(translator, UserErrorMessages.UserPasswordRequired)
            .Must((_, x) => x!.IsValidPassword())
            .WithTranslatedError(translator, UserErrorMessages.UserPasswordWeak);

        RuleFor(x => x.Gender)
            .IsInEnum()
            .When(x => x.Gender != null, ApplyConditionTo.CurrentValidator)
            .WithTranslatedError(translator, UserErrorMessages.UserGenderNotInEnum);
    }

    protected sealed override void ApplyRules(IMessageTranslatorService translator) { }

    private async Task<bool> IsUsernameAvailableAsync(
        string username,
        Ulid? id = null,
        CancellationToken cancellationToken = default
    ) =>
        !await unitOfWork
            .Repository<User>()
            .AnyAsync(
                x => (!id.HasValue && x.Username == username) || x.Username == username,
                cancellationToken
            );
}
