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

        /// <summary>
        /// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="repository">repository</param>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>A <see cref="Task{TEntity}"/> that represents the asynchronous find operation. The task result contains the found entity or null.</returns>
        public static Task<TEntity> FindAsync<TDbContext, TEntity>(this IEFRepository<TDbContext, TEntity> repository,
            params object[] keyValues)
            where TDbContext : DbContext
            where TEntity : class
        {
            return repository.DbContext.FindAsync<TEntity>(keyValues, CancellationToken.None);
        }
    }
}
