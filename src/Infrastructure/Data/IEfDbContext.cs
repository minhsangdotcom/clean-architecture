using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Data;

public interface IEfDbContext : IDisposable
{
    EntityEntry Entry(object entity);

    public DbSet<TEntity> Set<TEntity>()
        where TEntity : class;

    public DatabaseFacade DatabaseFacade { get; }

    Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default
    );

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
