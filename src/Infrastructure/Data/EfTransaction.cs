using Application.Common.Interfaces.UnitOfWorks;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Data;

public class EfTransaction(IDbContextTransaction transaction) : ITransaction
{
    public Task CommitAsync(CancellationToken cancellationToken = default) =>
        transaction.CommitAsync(cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken = default) =>
        transaction.RollbackAsync(cancellationToken);

    public async Task DisposeAsync()
    {
        if (transaction != null)
        {
            await transaction.DisposeAsync();
            transaction = null!;
        }
    }
}
