using System;
using System.Collections.Generic;
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
            return repository.Get(queryBuilder =>
                queryBuilder.WithPredict(predicate)
                .WithInclude(include)
                .WithOrderBy(orderBy)
                .WithNoTracking(disableTracking)
                .IgnoreQueryFilters(ignoreQueryFilter)
                .WithCount(1))
                .FirstOrDefault();
        }

        public static TResult Fetch<TDbContext, TEntity, TResult>(this EFRepository<TDbContext, TEntity> repository, Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false)
            where TDbContext : DbContext
            where TEntity : class
        {
            return repository.Get(selector, queryBuilder =>
                queryBuilder.WithPredict(predicate)
                    .WithInclude(include)
                    .WithOrderBy(orderBy)
                    .WithNoTracking(disableTracking)
                    .IgnoreQueryFilters(ignoreQueryFilter)
                    .WithCount(1)).FirstOrDefault();
        }

        public static Task<TEntity> FetchAsync<TDbContext, TEntity>(this EFRepository<TDbContext, TEntity> repository, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false, CancellationToken cancellationToken = default)
            where TDbContext : DbContext
            where TEntity : class
        {
            return repository.GetAsync(queryBuilder =>
                queryBuilder.WithPredict(predicate)
                    .WithInclude(include)
                    .WithOrderBy(orderBy)
                    .WithNoTracking(disableTracking)
                    .IgnoreQueryFilters(ignoreQueryFilter)
                    .WithCount(1), cancellationToken).ContinueWith(r => r.Result.FirstOrDefault(), cancellationToken);
        }

        public static Task<TResult> FetchAsync<TDbContext, TEntity, TResult>(this EFRepository<TDbContext, TEntity> repository, Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false, CancellationToken cancellationToken = default)
            where TDbContext : DbContext
            where TEntity : class
        {
            return repository.GetAsync(selector, queryBuilder =>
                queryBuilder.WithPredict(predicate)
                    .WithInclude(include)
                    .WithOrderBy(orderBy)
                    .WithNoTracking(disableTracking)
                    .IgnoreQueryFilters(ignoreQueryFilter)
                    .WithCount(1), cancellationToken).ContinueWith(r => r.Result.FirstOrDefault(), cancellationToken);
        }

        public static List<TEntity> Top<TDbContext, TEntity>(this EFRepository<TDbContext, TEntity> repository, int count, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false)
        where TDbContext : DbContext
        where TEntity : class
        {
            if (count <= 0) count = 10;

            return repository.Get(queryBuilder =>
                queryBuilder.WithPredict(predicate)
                    .WithInclude(include)
                    .WithOrderBy(orderBy)
                    .WithNoTracking(disableTracking)
                    .IgnoreQueryFilters(ignoreQueryFilter)
                    .WithCount(count));
        }

        public static List<TResult> Top<TDbContext, TEntity, TResult>(this EFRepository<TDbContext, TEntity> repository, int count, Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false)
            where TDbContext : DbContext
            where TEntity : class
        {
            if (count <= 0) count = 10;

            return repository.Get(selector, queryBuilder =>
                queryBuilder.WithPredict(predicate)
                    .WithInclude(include)
                    .WithOrderBy(orderBy)
                    .WithNoTracking(disableTracking)
                    .IgnoreQueryFilters(ignoreQueryFilter)
                    .WithCount(count));
        }

        public static Task<List<TEntity>> TopAsync<TDbContext, TEntity>(this EFRepository<TDbContext, TEntity> repository, int count, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false, CancellationToken cancellationToken = default)
            where TDbContext : DbContext
            where TEntity : class
        {
            if (count <= 0) count = 10;

            return repository.GetAsync(queryBuilder =>
                queryBuilder.WithPredict(predicate)
                    .WithInclude(include)
                    .WithOrderBy(orderBy)
                    .WithNoTracking(disableTracking)
                    .IgnoreQueryFilters(ignoreQueryFilter)
                    .WithCount(count), cancellationToken);
        }

        public static Task<List<TResult>> TopAsync<TDbContext, TEntity, TResult>(this EFRepository<TDbContext, TEntity> repository, int count, Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false, CancellationToken cancellationToken = default)
            where TDbContext : DbContext
            where TEntity : class
        {
            if (count <= 0) count = 10;

            return repository.GetAsync(selector, queryBuilder =>
                queryBuilder.WithPredict(predicate)
                    .WithInclude(include)
                    .WithOrderBy(orderBy)
                    .WithNoTracking(disableTracking)
                    .IgnoreQueryFilters(ignoreQueryFilter)
                    .WithCount(count), cancellationToken);
        }

        public static List<TEntity> Get<TDbContext, TEntity>(this EFRepository<TDbContext, TEntity> repository, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false)
        where TDbContext : DbContext
        where TEntity : class
        {
            return repository.Get(queryBuilder =>
                queryBuilder.WithPredict(predicate)
                    .WithInclude(include)
                    .WithOrderBy(orderBy)
                    .WithNoTracking(disableTracking)
                    .IgnoreQueryFilters(ignoreQueryFilter));
        }

        public static List<TResult> Get<TDbContext, TEntity, TResult>(this EFRepository<TDbContext, TEntity> repository, Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false)
            where TDbContext : DbContext
            where TEntity : class
        {
            return repository.Get(selector, queryBuilder =>
                queryBuilder.WithPredict(predicate)
                    .WithInclude(include)
                    .WithOrderBy(orderBy)
                    .WithNoTracking(disableTracking)
                    .IgnoreQueryFilters(ignoreQueryFilter));
        }

        public static Task<List<TEntity>> GetAsync<TDbContext, TEntity>(this EFRepository<TDbContext, TEntity> repository, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false, CancellationToken cancellationToken = default)
            where TDbContext : DbContext
            where TEntity : class
        {
            return repository.GetAsync(queryBuilder =>
                queryBuilder.WithPredict(predicate)
                    .WithInclude(include)
                    .WithOrderBy(orderBy)
                    .WithNoTracking(disableTracking)
                    .IgnoreQueryFilters(ignoreQueryFilter), cancellationToken);
        }

        public static Task<List<TResult>> GetAsync<TDbContext, TEntity, TResult>(this EFRepository<TDbContext, TEntity> repository, Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false, CancellationToken cancellationToken = default)
            where TDbContext : DbContext
            where TEntity : class
        {
            return repository.GetAsync(selector, queryBuilder =>
                queryBuilder.WithPredict(predicate)
                    .WithInclude(include)
                    .WithOrderBy(orderBy)
                    .WithNoTracking(disableTracking)
                    .IgnoreQueryFilters(ignoreQueryFilter), cancellationToken);
        }
    }
}
