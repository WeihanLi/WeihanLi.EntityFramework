using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WeihanLi.EntityFramework
{
    public class EFUnitOfWork<TDbContext> : IEFUnitOfWork<TDbContext> where TDbContext : DbContext
    {
        public EFUnitOfWork(TDbContext dbContext)
        {
            DbContext = dbContext;
            if (DbContext.IsRelationalDatabase())
            {
                DbContext.Database.BeginTransaction();
            }
        }

        public TDbContext DbContext { get; }

        public virtual void Commit()
        {
            DbContext.Database.CurrentTransaction?.Commit();
            DbContext.SaveChanges();
        }

        public virtual Task CommitAsync(CancellationToken cancellationToken)
        {
            DbContext.Database.CurrentTransaction?.Commit();
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual void Rollback()
        {
            DbContext.Database.CurrentTransaction?.Rollback();
        }

        public virtual Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            DbContext.Database.CurrentTransaction?.Rollback();
            return Task.CompletedTask;
        }

        public virtual void Dispose()
        {
            DbContext.Database.CurrentTransaction?.Dispose();
        }
    }
}
