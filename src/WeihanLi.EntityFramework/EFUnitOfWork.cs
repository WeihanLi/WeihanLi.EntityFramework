using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WeihanLi.EntityFramework
{
    public class EFUnitOfWork<TDbContext> : IEFUnitOfWork<TDbContext> where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;

        public EFUnitOfWork(TDbContext dbContext)
        {
            _dbContext = dbContext;
            if (_dbContext.IsRelationalDatabase())
            {
                _dbContext.Database.BeginTransaction();
            }
        }

        public TDbContext DbContext => _dbContext;

        public virtual void Commit()
        {
            _dbContext.Database.CurrentTransaction?.Commit();
            _dbContext.SaveChanges();
        }

        public virtual Task CommitAsync(CancellationToken cancellationToken)
        {
            _dbContext.Database.CurrentTransaction?.Commit();
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual void Rollback()
        {
            _dbContext.Database.CurrentTransaction?.Rollback();
        }

        public virtual Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            _dbContext.Database.CurrentTransaction?.Rollback();
            return Task.CompletedTask;
        }

        public virtual void Dispose()
        {
            _dbContext.Database.CurrentTransaction?.Dispose();
        }
    }
}
