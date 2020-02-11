using Microsoft.EntityFrameworkCore;

namespace WeihanLi.EntityFramework
{
    public interface IEFRepositoryFactory<TDbContext> where TDbContext : DbContext
    {
        IEFRepository<TDbContext, TEntity> GetRepository<TEntity>() where TEntity : class;
    }
}
