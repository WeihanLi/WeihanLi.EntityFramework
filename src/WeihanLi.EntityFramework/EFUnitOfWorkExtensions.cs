using System;
using Microsoft.EntityFrameworkCore;

namespace WeihanLi.EntityFramework
{
    public static class EFUnitOfWorkExtensions
    {
        public static DbSet<TEntity> DbSet<TEntity>(this IEFUnitOfWork<DbContext> unitOfWork)
            where TEntity : class
        {
            if (null == unitOfWork)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }
            return unitOfWork.DbContext.Set<TEntity>();
        }
    }
}
