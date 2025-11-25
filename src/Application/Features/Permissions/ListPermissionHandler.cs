using Application.Common.Interfaces.DbContexts;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Permissions;
using Domain.Aggregates.Permissions;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Permissions;

public class ListPermissionHandler(
    IEfDbContext efDbContext,
    PermissionDefinitionContext permissionDefinitionContext
) : IRequestHandler<ListPermissionQuery, Result<IReadOnlyList<ListGroupPermissionResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<ListGroupPermissionResponse>>> Handle(
        ListPermissionQuery request,
        CancellationToken cancellationToken
    )
    {
        var permissionGroup = await efDbContext
            .Set<Permission>()
            .OrderBy(x => x.Group)
            .GroupBy(x => x.Group)
            .Select(g => new ListGroupPermissionResponse
            {
                GroupName = g.Key,
                Permissions = g.OrderBy(x => x.Code)
                    .Select(p => new ListPermissionResponse
                    {
                        Id = p.Id,
                        Code = p.Code,
                        Name = p.Name,
                        CreatedAt = p.CreatedAt,
                    })
                    .ToList(),
            })
            .ToListAsync(cancellationToken);

        for (int i = 0; i < permissionGroup.Count; i++)
        {
            var group = permissionGroup[i];
            Dictionary<string, PermissionResponse>? dbPermissions = group.Permissions?.ToDictionary(
                p => p.Code!,
                p => new PermissionResponse
                {
                    Id = p.Id,
                    Code = p.Code!,
                    Name = p.Name!,
                    CreatedAt = p.CreatedAt,
                }
            );
            if (
                permissionDefinitionContext.Groups.TryGetValue(
                    group.GroupName!,
                    out var groupDefinition
                )
            )
            {
                for (int j = 0; j < group.Permissions?.Count; j++)
                {
                    PermissionDefinition? definition = groupDefinition.Permissions.Find(p =>
                        p.Code == group.Permissions[j].Code
                    );
                    ListPermissionResponse mapped = MapDefinitionToResponse(
                        definition,
                        dbPermissions
                    );
                    group.Permissions[j].Children = mapped.Children ?? [];
                }
            }
        }

        return Result<IReadOnlyList<ListGroupPermissionResponse>>.Success(permissionGroup);
    }

    private static ListPermissionResponse MapDefinitionToResponse(
        PermissionDefinition? definitionRoot,
        Dictionary<string, PermissionResponse>? dbPermissions
    )
    {
        if (
            dbPermissions?.TryGetValue(definitionRoot!.Code, out PermissionResponse? permission)
            is true
        )
        {
            List<ListPermissionResponse> children = [];
            if (definitionRoot?.Children?.Count > 0)
            {
                foreach (PermissionDefinition childDef in definitionRoot.Children)
                {
                    ListPermissionResponse child = MapDefinitionToResponse(childDef, dbPermissions);
                    children.Add(child);
                }
            }

            return new ListPermissionResponse
            {
                Id = permission?.Id ?? Ulid.Empty,
                CreatedAt = permission?.CreatedAt,
                Code = permission?.Code,
                Name = permission?.Name,
                Description = permission?.Description,
                Children = children,
            };
        }
        return new();
    }
}
