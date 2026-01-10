using Application.Common.Interfaces.Services.Localization;
using Application.Contracts.ApiWrapper;
using Application.Contracts.Permissions;
using Domain.Aggregates.Permissions;
using Mediator;
using PermissionKey = Application.Common.Interfaces.Services.Localization.Permissions;

namespace Application.Features.Permissions;

public class ListPermissionHandler(
    IPermissionRepository permissionRepository,
    PermissionDefinitionContext permissionDefinitionContext,
    ITranslator<PermissionKey> translator
) : IRequestHandler<ListPermissionQuery, Result<IReadOnlyList<ListGroupPermissionResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<ListGroupPermissionResponse>>> Handle(
        ListPermissionQuery request,
        CancellationToken cancellationToken
    )
    {
        List<IGrouping<string?, Permission>> groupPermission =
            await permissionRepository.ListAsync();

        var permissionGroup = groupPermission
            .Select(g => new ListGroupPermissionResponse
            {
                Name = g.Key,
                Permissions =
                [
                    .. g.OrderBy(x => x.Code)
                        .Select(p => new ListPermissionResponse
                        {
                            Id = p.Id,
                            Code = p.Code,
                            CreatedAt = p.CreatedAt,
                        }),
                ],
            })
            .ToList();

        for (int i = 0; i < permissionGroup.Count; i++)
        {
            ListGroupPermissionResponse group = permissionGroup[i];
            group.NameTranslation = translator.Translate(group.Name!);
            group.Permissions?.ForEach(p =>
                p.CodeTranslation = translator.Translate(p.Code! ?? "")
            );
            Dictionary<string, PermissionResponse>? dbPermissions = group.Permissions?.ToDictionary(
                p => p.Code!,
                p => new PermissionResponse
                {
                    Id = p.Id,
                    Code = p.Code!,
                    CreatedAt = p.CreatedAt,
                    CodeTranslation = p.CodeTranslation,
                }
            );
            if (
                permissionDefinitionContext.Groups.TryGetValue(group.Name!, out var groupDefinition)
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

    private ListPermissionResponse MapDefinitionToResponse(
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
                CodeTranslation = translator.Translate(permission?.Code ?? ""),
                Children = children,
            };
        }
        return new();
    }
}
