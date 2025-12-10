using Application.Common.Interfaces.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Infrastructure.Data;

public interface IEfDbContext : IDisposable
{
    EntityEntry Entry(object entity);

    public DbSet<TEntity> Set<TEntity>()
        where TEntity : class;

    public DatabaseFacade DatabaseFacade { get; }

    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
