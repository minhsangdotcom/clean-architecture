using Application.Contracts.ApiWrapper;
using Domain.Aggregates.Users.Enums;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Users.Commands.Profiles;

public class UpdateUserProfileCommand : IRequest<Result<UpdateUserProfileResponse>>
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public IFormFile? Avatar { get; set; }

    public Gender? Gender { get; set; }
}
