using Application.Common.Constants;
using Application.Features.Roles.Commands.Update;
using Application.SubcutaneousTests.Extensions;
using AutoFixture;
using Domain.Aggregates.Roles;
using DotNetCoreExtension.Extensions;
using SharedKernel.Common.Messages;
using Shouldly;

namespace Application.SubcutaneousTests.Roles.Commands.Update;

[Collection(nameof(TestingCollectionFixture))]
public class UpdateRoleHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
{
    [Fact]
    public async Task UpdateRole_WhenIdNotFound_ShouldReturnNotFoundException() { }

    [Fact]
    public async Task UpdateRole_WhenNoRoleClaims_ShouldUpdateRole() { }

    [Fact]
    public async Task UpdateRole_WhenNoDescription_ShouldUpdateRole() { }

    [Fact]
    public async Task UpdateRole_ShouldUpdateRole() { }

    public async Task InitializeAsync() { }

    public async Task DisposeAsync() => await Task.CompletedTask;
}
