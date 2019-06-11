using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace WeihanLi.EntityFramework
{
    public static class EFRepositoryExtensions
    {
        public static TEntity Fetch<TDbContext, TEntity>(this EFRepository<TDbContext, TEntity> repository, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false)
            where TDbContext : DbContext
            where TEntity : class
        {
            return repository.Top(1, predicate, orderBy, include, disableTracking, ignoreQueryFilter).FirstOrDefault();
        }

        public static TResult Fetch<TDbContext, TEntity, TResult>(this EFRepository<TDbContext, TEntity> repository, Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false)
            where TDbContext : DbContext
            where TEntity : class
        {
            return repository.Top(1, selector, predicate, orderBy, include, disableTracking, ignoreQueryFilter).FirstOrDefault();
        }

        public static Task<TEntity> FetchAsync<TDbContext, TEntity>(this EFRepository<TDbContext, TEntity> repository, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false, CancellationToken cancellationToken = default)
            where TDbContext : DbContext
            where TEntity : class
        {
            return repository.TopAsync(1, predicate, orderBy, include, disableTracking, ignoreQueryFilter, cancellationToken).ContinueWith(r => r.Result.FirstOrDefault(), cancellationToken);
        }

        public static Task<TResult> FetchAsync<TDbContext, TEntity, TResult>(this EFRepository<TDbContext, TEntity> repository, Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false, CancellationToken cancellationToken = default)
            where TDbContext : DbContext
            where TEntity : class
        {
            return repository.TopAsync(1, selector, predicate, orderBy, include, disableTracking, ignoreQueryFilter, cancellationToken).ContinueWith(r => r.Result.FirstOrDefault(), cancellationToken);
        }
    }
}
