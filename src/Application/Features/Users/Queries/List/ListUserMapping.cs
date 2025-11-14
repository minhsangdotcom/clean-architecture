using System.Linq.Expressions;
using Domain.Aggregates.Users;

namespace Application.Features.Users.Queries.List;

public static class ListUserMapping
{
    public static Expression<Func<User, ListUserResponse>> Selector() =>
        user => new ListUserResponse()
        {
            Id = user.Id,
            CreatedAt = user.CreatedAt,
            CreatedBy = user.CreatedBy,
            UpdatedAt = user.UpdatedAt,
            UpdatedBy = user.UpdatedBy,

            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DayOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Avatar = user.Avatar,
            Status = user.Status,
        };
}
