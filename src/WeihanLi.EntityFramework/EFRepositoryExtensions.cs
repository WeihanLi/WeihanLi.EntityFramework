using System;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace WeihanLi.EntityFramework
{
    public static class EFRepositoryExtensions
    {
        public static Task<int> UpdateAsync<TDbContext, TEntity>([NotNull] this IEFRepository<TDbContext, TEntity> repository,
            TEntity entity, CancellationToken cancellationToken)
            where TDbContext : DbContext
            where TEntity : class
            => repository.UpdateAsync(entity, Array.Empty<string>(), cancellationToken);

        public static Task<int> UpdateAsync<TDbContext, TEntity>([NotNull] this IEFRepository<TDbContext, TEntity> repository,
            TEntity entity,
            params Expression<Func<TEntity, object>>[] propertyExpressions)
            where TDbContext : DbContext
            where TEntity : class
            => repository.UpdateAsync(entity, propertyExpressions);

        public static Task<int> UpdateWithoutAsync<TDbContext, TEntity>([NotNull] this IEFRepository<TDbContext, TEntity> repository,
            TEntity entity,
            params Expression<Func<TEntity, object>>[] propertyExpressions)
            where TDbContext : DbContext
            where TEntity : class
            => repository.UpdateWithoutAsync(entity, propertyExpressions);

        public static ValueTask<TEntity> FindAsync<TDbContext, TEntity>([NotNull] this IEFRepository<TDbContext, TEntity> repository,
            params object[] keyValues)
            where TDbContext : DbContext
            where TEntity : class
        {
            return repository.FindAsync(keyValues, CancellationToken.None);
        }

        public static Task<int> DeleteAsync<TDbContext, TEntity>([NotNull] this IEFRepository<TDbContext, TEntity> repository,
            params object[] keyValues)
            where TDbContext : DbContext
            where TEntity : class
        {
            return repository.DeleteAsync(keyValues, CancellationToken.None);
        }

        public static IEFUnitOfWork<TDbContext> GetUnitOfWork<TDbContext, TEntity>(
            [NotNull]this IEFRepository<TDbContext, TEntity> repository)
            where TDbContext : DbContext
            where TEntity : class
        {
            return new EFUnitOfWork<TDbContext>(repository.DbContext);
        }

        
        public static IEFUnitOfWork<TDbContext> GetUnitOfWork<TDbContext, TEntity>(
            [NotNull]this IEFRepository<TDbContext, TEntity> repository,
            IsolationLevel isolationLevel
            )
            where TDbContext : DbContext
            where TEntity : class
        {
            return new EFUnitOfWork<TDbContext>(repository.DbContext, isolationLevel);
        }
    }
}
