using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace WeihanLi.EntityFramework
{
    public class EFUnitOfWork<TDbContext> : IEFUnitOfWork<TDbContext> where TDbContext : DbContext
    {
        // https://docs.microsoft.com/en-us/ef/core/saving/transactions
        private readonly IDbContextTransaction _transaction = null;

        public EFUnitOfWork(TDbContext dbContext)
        {
            DbContext = dbContext;
            if (DbContext.IsRelationalDatabase())
            {
                _transaction = DbContext.Database.BeginTransaction();
            }
        }

        public TDbContext DbContext { get; }

        public virtual void Commit()
        {
            DbContext.SaveChanges();
            _transaction?.Commit();
        }

        public virtual async Task CommitAsync(CancellationToken cancellationToken)
        {
            await DbContext.SaveChangesAsync(cancellationToken);
            _transaction?.Commit();
        }

        public virtual void Rollback()
        {
            _transaction?.Rollback();
        }

        public virtual Task RollbackAsync(CancellationToken cancellationToken)
        {
            _transaction?.Rollback();
            return Task.CompletedTask;
        }

        public virtual void Dispose()
        {
            _transaction?.Dispose();
        }
    }
}
