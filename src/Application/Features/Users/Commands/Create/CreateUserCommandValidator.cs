using Application.Common.ErrorCodes;
using Application.Common.Extensions;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.SharedFeatures.Validators.Users;
using Domain.Aggregates.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Application.Features.Users.Commands.Create;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly IEfUnitOfWork unitOfWork;
    private readonly IStringLocalizer stringLocalizer;

    public CreateUserCommandValidator(IEfUnitOfWork unitOfWork, IStringLocalizer stringLocalizer)
    {
        this.unitOfWork = unitOfWork;
        this.stringLocalizer = stringLocalizer;
        ApplyRules();
    }

    private void ApplyRules()
    {
        Include(new UserValidator(unitOfWork, stringLocalizer)!);
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserUsernameRequired,
                stringLocalizer[UserErrorMessages.UserUsernameRequired]
            ))
            .Must((_, x) => x!.IsValidUsername())
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserUsernameInvalid,
                stringLocalizer[UserErrorMessages.UserUsernameInvalid]
            ))
            .MustAsync((username, ct) => IsUsernameAvailableAsync(username!, cancellationToken: ct))
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserUsernameExistent,
                stringLocalizer[UserErrorMessages.UserUsernameExistent]
            ));

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserEmailRequired,
                stringLocalizer[UserErrorMessages.UserEmailRequired]
            ))
            .Must(x => x!.IsValidEmail())
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserEmailInvalid,
                stringLocalizer[UserErrorMessages.UserEmailInvalid]
            ))
            .MustAsync((email, ct) => IsEmailAvailableAsync(email!, cancellationToken: ct))
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserEmailExistent,
                stringLocalizer[UserErrorMessages.UserEmailExistent]
            ));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserPasswordRequired,
                stringLocalizer[UserErrorMessages.UserPasswordRequired]
            ))
            .Must((_, x) => x!.IsValidPassword())
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserPasswordWeak,
                stringLocalizer[UserErrorMessages.UserPasswordWeak]
            ));

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithState(_ => new ErrorReason(
                UserErrorMessages.UserGenderNotInEnum,
                stringLocalizer[UserErrorMessages.UserGenderNotInEnum]
            ));
    }

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

    private async Task<bool> IsEmailAvailableAsync(
        string email,
        Ulid? id = null,
        CancellationToken cancellationToken = default
    ) =>
        !await unitOfWork
            .Repository<User>()
            .AnyAsync(
                x => x.Email == email && (!id.HasValue || x.Id != id.Value),
                cancellationToken
            );
}
