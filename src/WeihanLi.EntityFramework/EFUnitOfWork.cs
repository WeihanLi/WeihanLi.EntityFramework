using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace WeihanLi.EntityFramework
{
    public class EFUnitOfWork<TDbContext> : IEFUnitOfWork<TDbContext>, IDisposable where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;
        private readonly IDbContextTransaction _dbTransaction = null;

        public EFUnitOfWork(TDbContext dbContext)
        {
            _dbContext = dbContext;
            if (_dbContext.IsRelational())
            {
                _dbTransaction = _dbContext.Database.BeginTransaction();
            }
        }

        public TDbContext DbContext => _dbContext;

        public virtual void Commit()
        {
            _dbTransaction?.Commit();
            _dbContext.SaveChanges();
        }

        public virtual Task CommitAsync(CancellationToken cancellationToken)
        {
            _dbTransaction?.Commit();
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual void Rollback()
        {
            _dbTransaction?.Rollback();
        }

        public virtual Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            _dbTransaction?.Rollback();
            return Task.CompletedTask;
        }

        public virtual void Dispose()
        {
            _dbTransaction?.Dispose();
        }
    }
}
