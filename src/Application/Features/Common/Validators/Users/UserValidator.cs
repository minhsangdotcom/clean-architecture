using Application.Common.Extensions;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Requests.Users;
using Domain.Aggregates.Permissions;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using FluentValidation;
using SharedKernel.Common.Messages;

namespace Application.Features.Common.Validators.Users;

public partial class UserValidator : AbstractValidator<UserUpsertCommand>
{
    private readonly IEfUnitOfWork unitOfWork;

    public UserValidator(IEfUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
        ApplyRules();
    }

    private void ApplyRules()
    {
        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithState(x =>
                Messenger
                    .Create<User>()
                    .Property(x => x.LastName)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .MaximumLength(256)
            .WithState(x =>
                Messenger
                    .Create<User>()
                    .Property(x => x.LastName)
                    .Message(MessageType.MaximumLength)
                    .Build()
            );

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithState(x =>
                Messenger
                    .Create<User>()
                    .Property(x => x.FirstName)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .MaximumLength(256)
            .WithState(x =>
                Messenger
                    .Create<User>()
                    .Property(x => x.FirstName)
                    .Message(MessageType.MaximumLength)
                    .Build()
            );
        RuleFor(x => x.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .Must(x => x!.IsValidPhoneNumber())
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithState(x =>
                Messenger
                    .Create<User>()
                    .Property(x => x.PhoneNumber!)
                    .Message(MessageType.Valid)
                    .Negative()
                    .Build()
            );

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithState(x =>
                Messenger
                    .Create<UserUpsertCommand>(nameof(User))
                    .Property(x => x.Status!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .IsInEnum()
            .WithState(x =>
                Messenger
                    .Create<UserUpsertCommand>(nameof(User))
                    .Property(x => x.Status!)
                    .Negative()
                    .Message(MessageType.AmongTheAllowedOptions)
                    .Build()
            );

        RuleFor(x => x.Roles)
            .NotEmpty()
            .WithState(x =>
                Messenger
                    .Create<UserUpsertCommand>(nameof(User))
                    .Property(x => x.Roles!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            )
            .Must(x => x!.Distinct().Count() == x!.Count)
            .WithState(x =>
                Messenger
                    .Create<UserUpsertCommand>(nameof(User))
                    .Property(x => x.Roles!)
                    .Message(MessageType.Unique)
                    .Negative()
                    .Build()
            )
            .MustAsync((roles, _) => IsRolesAvailableAsync(roles!))
            .WithState(x =>
                Messenger
                    .Create<UserUpsertCommand>(nameof(User))
                    .Property(x => x.Roles!)
                    .Message(MessageType.Found)
                    .Negative()
                    .Build()
            );

        When(
            x => x.Permissions != null,
            () =>
            {
                RuleFor(r => r.Permissions)
                    .Must((p, _) => p.Permissions!.Distinct().Count() == p.Permissions!.Count)
                    .WithState(req =>
                        Messenger
                            .Create<User>()
                            .Property(req => req.Permissions)
                            .Message(MessageType.Unique)
                            .Negative()
                            .BuildMessage()
                    )
                    .MustAsync((m, _) => IsPermissionsAvailableAsync(m!))
                    .WithState(req =>
                        Messenger
                            .Create<UserUpsertCommand>(nameof(User))
                            .Property(req => req.Roles!)
                            .Message(MessageType.Found)
                            .Negative()
                            .Build()
                    );
            }
        );
    }

    private async Task<bool> IsRolesAvailableAsync(IEnumerable<Ulid> roles) =>
        await unitOfWork.Repository<Role>().CountAsync(x => roles.Contains(x.Id)) == roles.Count();

    private async Task<bool> IsPermissionsAvailableAsync(IEnumerable<Ulid> permissions) =>
        await unitOfWork.Repository<Permission>().CountAsync(x => permissions.Contains(x.Id))
        == permissions.Count();
}
