using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Permissions;

public class ListPermissionHandler(IEfUnitOfWork unitOfWork)
    : IRequestHandler<ListPermissionQuery, Result<IEnumerable<ListPermissionResponse>>>
{
    public async ValueTask<Result<IEnumerable<ListPermissionResponse>>> Handle(
        ListPermissionQuery request,
        CancellationToken cancellationToken
    )
    {
        return Result<IEnumerable<ListPermissionResponse>>.Success([]);
    }
}
