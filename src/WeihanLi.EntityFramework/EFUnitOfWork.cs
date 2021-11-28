using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace WeihanLi.EntityFramework
{
    public class EFUnitOfWork<TDbContext> :
        IEFUnitOfWork<TDbContext> where TDbContext : DbContext
        , IDisposable
    {
        // https://docs.microsoft.com/en-us/ef/core/saving/transactions
        private readonly IDbContextTransaction? _transaction;

        public EFUnitOfWork(TDbContext dbContext)
        {
            DbContext = dbContext;
            if (DbContext.Database.IsRelational())
            {
                _transaction = DbContext.Database.BeginTransaction();
            }
        }

        internal EFUnitOfWork(TDbContext dbContext, IsolationLevel isolationLevel)
        {
            DbContext = dbContext;
            if (DbContext.Database.IsRelational())
            {
                _transaction = DbContext.Database.BeginTransaction(isolationLevel);
            }
        }

        public TDbContext DbContext { get; }

        public IEFRepository<TDbContext, TEntity> GetRepository<TEntity>() where TEntity : class
        {
            return new EFRepository<TDbContext, TEntity>(DbContext);
        }

        public virtual void Commit()
        {
            DbContext.SaveChanges();
            _transaction?.Commit();
        }

        public virtual async Task CommitAsync(CancellationToken cancellationToken)
        {
            await DbContext.SaveChangesAsync(cancellationToken);
            if (_transaction is not null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
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
