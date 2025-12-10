namespace Application.Common.Interfaces.UnitOfWorks;

public interface ITransaction
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
    Task DisposeAsync();
}
