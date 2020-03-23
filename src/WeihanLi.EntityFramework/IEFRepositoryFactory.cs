using Microsoft.EntityFrameworkCore;

namespace WeihanLi.EntityFramework
{
    public interface IEFRepositoryFactory<out TDbContext> where TDbContext : DbContext
    {
        IEFRepository<TDbContext, TEntity> GetRepository<TEntity>() where TEntity : class;
    }
}
