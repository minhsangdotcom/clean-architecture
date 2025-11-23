using Api.common.EndpointConfigurations;
using Api.common.Results;
using Api.common.Routers;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Cache;
using Application.Contracts.ApiWrapper;
using Application.Features.Users.Commands.Profiles;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace Api.Endpoints.User;

public class UpdateUserProfileEndpoint : IEndpoint
{
    public EndpointVersion Version => EndpointVersion.One;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(Router.UserRoute.Profile, HandleAsync)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Update user profile 🛠️ 👨 📋",
                Description = "Updates profile information for the currently authenticated user.",
                Tags = [new OpenApiTag() { Name = Router.UserRoute.Tags }],
            })
            .WithRequestValidation<UpdateUserProfileCommand>()
            .Authorize()
            .DisableAntiforgery();
    }

    private async Task<
        Results<Ok<ApiResponse<UpdateUserProfileResponse>>, ProblemHttpResult>
    > HandleAsync(
        [FromForm] UpdateUserProfileCommand request,
        [FromServices] ISender sender,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IMemoryCacheService cacheService,
        CancellationToken cancellationToken = default
    )
    {
        Ulid? userId = currentUser.Id;
        string key = $"{nameof(GetUserProfileEndpoint)}:{userId}";
        bool isExisted = cacheService.HasKey(key);
        if (isExisted)
        {
            cacheService.Remove(key);
        }
        var result = await sender.Send(request, cancellationToken);
        return result.ToResult();
    }
}
