using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WeihanLi.EntityFramework
{
    public interface IEFUnitOfWork<out TDbContext> where TDbContext : DbContext
    {
        TDbContext DbContext { get; }

        int Commit();

        Task<int> CommitAsync(CancellationToken cancellationToken = default);
    }
}
