using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WeihanLi.EntityFramework
{
    public interface IEFUnitOfWork<TDbContext> where TDbContext : DbContext
    {
        TDbContext DbContext { get; }

        DbSet<TEntity> DbSet<TEntity>() where TEntity : class;

        int Commit();

        Task<int> CommitAsync(CancellationToken cancellationToken = default);
    }
}
