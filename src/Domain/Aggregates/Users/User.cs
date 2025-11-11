using System.ComponentModel.DataAnnotations.Schema;
using Ardalis.GuardClauses;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Events;
using Domain.Aggregates.Users.ValueObjects;
using Domain.Common;
using DotNetCoreExtension.Extensions.Reflections;
using Mediator;
using SharedKernel.Constants;

namespace Domain.Aggregates.Users;

public class User : AggregateRoot
{
    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string Username { get; private set; }

    public string Password { get; private set; }

    public string Email { get; private set; }

    public string PhoneNumber { get; set; }

    public DateTime? DayOfBirth { get; set; }

    public Gender? Gender { get; set; }

    public Address? Address { get; private set; }

    public string? Avatar { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public ICollection<UserClaim>? UserClaims { get; set; } = [];

    public ICollection<UserRole>? UserRoles { get; set; } = [];

    public ICollection<UserToken>? UserTokens { get; set; } = [];

    public ICollection<UserResetPassword>? UserResetPasswords { get; set; } = [];

    // default user claim are ready to update into db
    [NotMapped]
    public IReadOnlyCollection<UserClaim> DefaultUserClaimsToUpdates { get; private set; } = [];

    public User(
        string firstName,
        string lastName,
        string username,
        string password,
        string email,
        string phoneNumber,
        Address? address = null
    )
    {
        FirstName = Guard.Against.NullOrEmpty(firstName, nameof(FirstName));
        LastName = Guard.Against.Null(lastName, nameof(LastName));
        Username = Guard.Against.Null(username, nameof(Username));
        Password = Guard.Against.Null(password, nameof(Password));
        Email = Guard.Against.Null(email, nameof(Email));
        PhoneNumber = Guard.Against.Null(phoneNumber, nameof(PhoneNumber));
        Address = address;
    }

    private User()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Username = string.Empty;
        Password = string.Empty;
        Email = string.Empty;
        PhoneNumber = string.Empty;
    }

    public void SetPassword(string password) =>
        Password = Guard.Against.NullOrWhiteSpace(password, nameof(password));

    public void Update(
        string? firstName,
        string? lastName,
        string? email,
        string? phoneNumber,
        DateTime? dayOfBirth
    )
    {
        if (!string.IsNullOrWhiteSpace(firstName))
        {
            FirstName = firstName;
        }
        if (!string.IsNullOrWhiteSpace(lastName))
        {
            LastName = lastName;
        }
        if (!string.IsNullOrWhiteSpace(email))
        {
            Email = email;
        }
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            PhoneNumber = phoneNumber;
        }

        if (dayOfBirth != null)
        {
            DayOfBirth = dayOfBirth;
        }
    }

    public void UpdateAddress(Address address) => Address = address;

    public void UpdateDefaultUserClaims() =>
        Emit(new UpdateDefaultUserClaimEvent() { User = this });

    public void CreateDefaultUserClaims() => ApplyCreateDefaultUserClaim();

    private List<UserClaim> GetUserClaims(bool isCreated = false) =>
        [
            new()
            {
                ClaimType = ClaimTypes.GivenName,
                ClaimValue = this.GetValue(x => x.FirstName!),
                UserId = isCreated ? Ulid.Empty : Id,
            },
            new()
            {
                ClaimType = ClaimTypes.FamilyName,
                ClaimValue = this.GetValue(x => x.LastName!),
                UserId = isCreated ? Ulid.Empty : Id,
            },
            new()
            {
                ClaimType = ClaimTypes.PreferredUsername,
                ClaimValue = this.GetValue(x => x.Username!),
                UserId = isCreated ? Ulid.Empty : Id,
            },
            new()
            {
                ClaimType = ClaimTypes.BirthDate,
                ClaimValue = this.GetValue(x => x.DayOfBirth!),
                UserId = isCreated ? Ulid.Empty : Id,
            },
            new()
            {
                ClaimType = ClaimTypes.Address,
                ClaimValue = this.GetValue(x => x.Address!),
                UserId = isCreated ? Ulid.Empty : Id,
            },
            new()
            {
                ClaimType = ClaimTypes.Picture,
                ClaimValue = this.GetValue(x => x.Avatar!),
                UserId = isCreated ? Ulid.Empty : Id,
            },
            new()
            {
                ClaimType = ClaimTypes.Gender,
                ClaimValue = this.GetValue(x => x.Gender!),
                UserId = isCreated ? Ulid.Empty : Id,
            },
            new()
            {
                ClaimType = ClaimTypes.Email,
                ClaimValue = this.GetValue(x => x.Email!),
                UserId = isCreated ? Ulid.Empty : Id,
            },
        ];

    private void ApplyUpdateDefaultUserClaim()
    {
        if (UserClaims == null || UserClaims.Count <= 0)
        {
            return;
        }

        UserClaim[] defaultClaims = [.. UserClaims.Where(x => x.Type == UserClaimType.Default)];
        Span<UserClaim> currentUserClaims = defaultClaims.AsSpan();

        List<UserClaim> userClaims = GetUserClaims();
        for (int i = 0; i < currentUserClaims.Length; i++)
        {
            UserClaim currentUserClaim = currentUserClaims[i];

            UserClaim? userClaim = userClaims.Find(x => x.ClaimType == currentUserClaim.ClaimType);
            if (userClaim == null)
            {
                continue;
            }

            currentUserClaim.ClaimValue = userClaim.ClaimValue;
        }

        DefaultUserClaimsToUpdates = defaultClaims;
    }

    private void ApplyCreateDefaultUserClaim()
    {
        if (UserClaims != null && UserClaims.Count > 0)
        {
            return;
        }
        UserClaims = GetUserClaims(true);
    }

    protected override bool TryApplyDomainEvent(INotification domainEvent)
    {
        switch (domainEvent)
        {
            case UpdateDefaultUserClaimEvent:
                ApplyUpdateDefaultUserClaim();
                return true;
            default:
                return false;
        }
    }
}
