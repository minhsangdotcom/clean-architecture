using Ardalis.GuardClauses;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Exceptions;
using SharedKernel.DomainEvents;
using SharedKernel.Entities;

namespace Domain.Aggregates.Users;

public class User : AggregateRoot
{
    #region Core Properties
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public Gender? Gender { get; private set; }
    public string? Avatar { get; private set; }
    public UserStatus Status { get; private set; } = UserStatus.Active;
    #endregion

    #region Navigation Collections
    public ICollection<UserClaim> Claims { get; private set; } = [];
    public ICollection<UserPermission> Permissions { get; private set; } = [];
    public ICollection<UserRole> Roles { get; private set; } = [];
    public ICollection<UserRefreshToken> RefreshTokens { get; private set; } = [];
    public ICollection<UserPasswordReset> PasswordResetRequests { get; private set; } = [];
    #endregion

    private User() { }

    public User(
        string firstName,
        string lastName,
        string username,
        string password,
        string email,
        string? phoneNumber = null,
        DateTime? dateOfBirth = null,
        Gender? gender = null,
        string? avatar = null
    )
    {
        FirstName = Guard.Against.NullOrEmpty(firstName, nameof(FirstName));
        LastName = Guard.Against.NullOrEmpty(lastName, nameof(LastName));
        Username = Guard.Against.NullOrEmpty(username, nameof(Username));
        Password = Guard.Against.NullOrEmpty(password, nameof(Password));
        Email = Guard.Against.NullOrEmpty(email, nameof(Email));
        PhoneNumber = phoneNumber;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Avatar = avatar;
    }

    public void InitializeIdentity(Ulid id, string createdBy)
    {
        Id = id;
        CreatedBy = createdBy;
    }

    public void HasPasswordAsync(string password) =>
        Password = Guard.Against.NullOrWhiteSpace(password, nameof(password));

    public void ChangePassword(string password) =>
        Password = Guard.Against.NullOrWhiteSpace(password, nameof(password));

    public void ChangeAvatar(string? avatar) => Avatar = avatar;

    public void Deactivate()
    {
        if (Status != UserStatus.Active)
        {
            throw new UserAlreadyInactiveException(Id);
        }
        Status = UserStatus.Inactive;
    }

    public void Activate()
    {
        if (Status != UserStatus.Inactive)
        {
            throw new UserAlreadyActiveException(Id);
        }
        Status = UserStatus.Inactive;
    }

    public void ClearAllRoles() => Roles.Clear();

    public void ClearAllPermissions() => Permissions.Clear();

    public User Update(
        string firstName,
        string lastName,
        string? phoneNumber,
        DateTime? dateOfBirth
    )
    {
        PhoneNumber = phoneNumber ?? PhoneNumber;
        DateOfBirth = dateOfBirth ?? DateOfBirth;
        FirstName = firstName ?? FirstName;
        LastName = lastName ?? LastName;
        return this;
    }

    public User UpdateProfile(
        string? firstName,
        string? lastName,
        Gender? gender = null,
        string? phoneNumber = null,
        DateTime? dateOfBirth = null
    )
    {
        PhoneNumber = phoneNumber ?? PhoneNumber;
        DateOfBirth = dateOfBirth ?? DateOfBirth;
        FirstName = firstName ?? FirstName;
        LastName = lastName ?? LastName;
        Gender = gender ?? Gender;

        return this;
    }

    protected override bool TryApplyDomainEvent(IDomainEvent domainEvent)
    {
        throw new NotImplementedException();
    }
}
