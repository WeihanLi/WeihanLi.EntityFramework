using Microsoft.EntityFrameworkCore;
using System;

namespace WeihanLi.EntityFramework
{
    public static class EFUnitOfWorkExtensions
    {
        public static DbSet<TEntity> DbSet<TEntity>(this IEFUnitOfWork<DbContext> unitOfWork)
            where TEntity : class
        {
            if (unitOfWork is null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }
            return unitOfWork.DbContext.Set<TEntity>();
        }

        public static IEFRepository<TDbContext, TEntity> GetRepository<TDbContext, TEntity>(
            this IEFUnitOfWork<TDbContext> unitOfWork)
            where TDbContext : DbContext
            where TEntity : class
        {
            if (unitOfWork is null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }
            return new EFRepository<TDbContext, TEntity>(unitOfWork.DbContext);
        }
    }
}
