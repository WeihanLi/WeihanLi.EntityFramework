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
        }

        public TDbContext DbContext => _dbContext;

        public DbSet<TEntity> DbSet<TEntity>() where TEntity : class
        {
            return _dbContext.Set<TEntity>();
        }

        public virtual int Commit()
        {
            return _dbContext.SaveChanges();
        }

        public Task<int> CommitAsync(CancellationToken cancellationToken)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
