using Application.Features.Common.Payloads.Roles;
using Domain.Aggregates.Roles;
using FluentValidation;
using SharedKernel.Common.Messages;

namespace Application.Features.Common.Validators.Roles;

public class RoleClaimValidator : AbstractValidator<RoleClaimPayload>
{
    public RoleClaimValidator()
    {
        RuleFor(x => x.ClaimType)
            .NotEmpty()
            .WithState(x =>
                Messenger
                    .Create<RoleClaim>(nameof(Role.Claims))
                    .Property(x => x.ClaimType!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            );

        RuleFor(x => x.ClaimValue)
            .NotEmpty()
            .WithState(x =>
                Messenger
                    .Create<RoleClaim>(nameof(Role.Claims))
                    .Property(x => x.ClaimValue!)
                    .Message(MessageType.Null)
                    .Negative()
                    .Build()
            );
    }
}
