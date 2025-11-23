using Application.Common.Extensions;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Messages;
using Application.Features.Common.Requests.Users;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Application.Features.Common.Validators.Users;

public partial class UserValidator : AbstractValidator<UserUpsertCommand>
{
    private readonly IEfUnitOfWork unitOfWork;
    private readonly IStringLocalizer stringLocalizer;

    public UserValidator(IEfUnitOfWork unitOfWork, IStringLocalizer stringLocalizer)
    {
        this.unitOfWork = unitOfWork;
        this.stringLocalizer = stringLocalizer;
        ApplyRules();
    }

    private void ApplyRules()
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

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.Status)
                    .WithError(MessageErrorType.Required)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            })
            .IsInEnum()
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<User>()
                    .Property(x => x.Status)
                    .Negative()
                    .WithError(MessageErrorType.AmongTheAllowedOptions)
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            });

        RuleFor(x => x.Roles)
            .NotEmpty()
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<UserUpsertCommand>(nameof(User))
                    .Property(x => x.Roles!)
                    .WithError(MessageErrorType.Required)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            })
            .Must(x => x!.Distinct().Count() == x!.Count)
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<UserUpsertCommand>(nameof(User))
                    .Property(x => x.Roles!)
                    .WithError(MessageErrorType.Unique)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            })
            .MustAsync(
                (roles, cancellationToken) => IsRolesAvailableAsync(roles!, cancellationToken)
            )
            .WithState(state =>
            {
                string errorMessage = Messenger
                    .Create<UserUpsertCommand>(nameof(User))
                    .Property(x => x.Roles!)
                    .WithError(MessageErrorType.Found)
                    .Negative()
                    .GetFullMessage();

                return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
            });

        When(
            x => x.Permissions != null,
            () =>
            {
                RuleFor(r => r.Permissions)
                    .Must((p, _) => p.Permissions!.Distinct().Count() == p.Permissions!.Count)
                    .WithState(state =>
                    {
                        string errorMessage = Messenger
                            .Create<UserUpsertCommand>(nameof(User))
                            .Property(req => req.Permissions!)
                            .WithError(MessageErrorType.Unique)
                            .Negative()
                            .GetFullMessage();

                        return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
                    })
                    .MustAsync(
                        (permissions, cancellationToken) =>
                            IsPermissionsAvailableAsync(permissions!, cancellationToken)
                    )
                    .WithState(state =>
                    {
                        string errorMessage = Messenger
                            .Create<UserUpsertCommand>(nameof(User))
                            .Property(req => req.Roles!)
                            .WithError(MessageErrorType.Found)
                            .Negative()
                            .GetFullMessage();

                        return new ErrorReason(errorMessage, stringLocalizer[errorMessage]);
                    });
            }
        );
    }

    private async Task<bool> IsRolesAvailableAsync(
        IEnumerable<Ulid> roles,
        CancellationToken cancellationToken = default
    ) =>
        await unitOfWork.Repository<Role>().CountAsync(x => roles.Contains(x.Id), cancellationToken)
        == roles.Count();

    private async Task<bool> IsPermissionsAvailableAsync(
        IEnumerable<Ulid> permissions,
        CancellationToken cancellationToken
    ) =>
        await unitOfWork
            .Repository<Permission>()
            .CountAsync(x => permissions.Contains(x.Id), cancellationToken) == permissions.Count();
}
