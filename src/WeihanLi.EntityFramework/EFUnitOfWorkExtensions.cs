using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace WeihanLi.EntityFramework
{
    public static class EFUnitOfWorkExtensions
    {
        public static DbSet<TEntity> DbSet<TEntity>([NotNull] this IEFUnitOfWork<DbContext> unitOfWork)
            where TEntity : class
        {
            if (null == unitOfWork)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }
            return unitOfWork.DbContext.Set<TEntity>();
        }

        public static IEFRepository<TDbContext, TEntity> GetRepository<TDbContext, TEntity>(
            [NotNull]this IEFUnitOfWork<TDbContext> unitOfWork)
            where TDbContext : DbContext
            where TEntity : class
        {
            return new EFRepository<TDbContext, TEntity>(unitOfWork.DbContext);
        }
    }
}
