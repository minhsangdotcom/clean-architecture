using Ardalis.GuardClauses;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Exceptions;
using Domain.Common;
using Mediator;

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
    public ICollection<UserClaim> Claims { get; set; } = [];
    public ICollection<UserPermission> Permissions { get; set; } = [];
    public ICollection<UserRole> Roles { get; set; } = [];
    public ICollection<UserRefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<UserPasswordReset> PasswordResetRequests { get; set; } = [];
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

    public void AssignRole(Ulid roleId)
    {
        if (Roles.Any(r => r.RoleId == roleId))
        {
            throw new RoleAlreadyAssignedException(Id, roleId);
        }

        Roles.Add(new UserRole { RoleId = roleId, UserId = Id });
    }

    public void DeleteRole(Ulid roleId)
    {
        var role =
            Roles.FirstOrDefault(r => r.RoleId == roleId)
            ?? throw new RoleNotAssignedException(Id, roleId);
        Roles.Remove(role);
    }

    public void ClearAllRoles()
    {
        Roles.Clear();
    }

    public void AssignPermission(Ulid permissionId)
    {
        if (Permissions.Any(p => p.PermissionId == permissionId))
            throw new PermissionAlreadyAssignedException(Id, permissionId);

        Permissions.Add(new UserPermission { PermissionId = permissionId, UserId = Id });
    }

    public void DeletePermission(Ulid permissionId)
    {
        var permission =
            Permissions.FirstOrDefault(p => p.PermissionId == permissionId)
            ?? throw new PermissionNotAssignedException(Id, permissionId);
        Permissions.Remove(permission);
    }

    public void ClearAllPermissions()
    {
        Permissions.Clear();
    }

    public User Update(
        string firstName,
        string lastName,
        string email,
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
        if (phoneNumber != null)
        {
            PhoneNumber = phoneNumber;
        }

        if (dayOfBirth != null)
        {
            DateOfBirth = dayOfBirth;
        }

        return this;
    }

    public void UpdateProfile(
        string? firstName,
        string? lastName,
        string? phoneNumber = null,
        DateTime? dateOfBirth = null,
        string? avatar = null
    )
    {
        PhoneNumber = phoneNumber ?? PhoneNumber;
        DateOfBirth = dateOfBirth ?? DateOfBirth;
        FirstName = firstName ?? FirstName;
        LastName = lastName ?? LastName;
        Avatar = avatar ?? Avatar;
    }

    protected override bool TryApplyDomainEvent(INotification domainEvent)
    {
        return true;
    }
}
