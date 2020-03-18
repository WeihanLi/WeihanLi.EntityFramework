using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WeihanLi.EntityFramework
{
    public static class EFRepositoryExtensions
    {
        public static Task<int> UpdateAsync<TDbContext, TEntity>(this IEFRepository<TDbContext, TEntity> repository,
            TEntity entity, CancellationToken cancellationToken)
            where TDbContext : DbContext
            where TEntity : class
            => repository.UpdateAsync(entity, Array.Empty<string>(), cancellationToken);

        public static Task<int> UpdateAsync<TDbContext, TEntity>(this IEFRepository<TDbContext, TEntity> repository,
            TEntity entity,
            params Expression<Func<TEntity, object>>[] propertyExpressions)
            where TDbContext : DbContext
            where TEntity : class
            => repository.UpdateAsync(entity, propertyExpressions);

        public static Task<int> UpdateWithoutAsync<TDbContext, TEntity>(this IEFRepository<TDbContext, TEntity> repository,
            TEntity entity,
            params Expression<Func<TEntity, object>>[] propertyExpressions)
            where TDbContext : DbContext
            where TEntity : class
            => repository.UpdateWithoutAsync(entity, propertyExpressions);

        public static Task<int> DeleteAsync<TDbContext, TEntity>(this IEFRepository<TDbContext, TEntity> repository,
            params object[] keyValues)
            where TDbContext : DbContext
            where TEntity : class
        {
            return repository.DeleteAsync(keyValues, CancellationToken.None);
        }
    }
}
