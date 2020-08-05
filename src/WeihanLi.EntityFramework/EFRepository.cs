using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeihanLi.Common.Models;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework
{
    public class EFRepository<TDbContext, TEntity> :
        IEFRepository<TDbContext, TEntity>
        where TDbContext : DbContext
        where TEntity : class
    {
        public TDbContext DbContext { get; }

        public EFRepository(TDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public TEntity Find(params object[] keyValues)
        {
            return DbContext.Find<TEntity>(keyValues);
        }

        public ValueTask<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken)
        {
            return DbContext.FindAsync<TEntity>(keyValues);
        }

        public int Delete(params object[] keyValues)
        {
            var entity = DbContext.Find<TEntity>(keyValues);
            if (null == entity)
            {
                return 0;
            }
            DbContext.Set<TEntity>().Remove(entity);
            return DbContext.SaveChanges();
        }

        public async Task<int> DeleteAsync(object[] keyValues, CancellationToken cancellationToken)
        {
            var entity = DbContext.Find<TEntity>(keyValues);
            if (null == entity)
            {
                return 0;
            }
            DbContext.Set<TEntity>().Remove(entity);
            return await DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual int Count(Expression<Func<TEntity, bool>> whereExpression) => DbContext.Set<TEntity>().Count(whereExpression);

        public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        => DbContext.Set<TEntity>().CountAsync(whereExpression, cancellationToken);

        public virtual long LongCount(Expression<Func<TEntity, bool>> whereExpression) => DbContext.Set<TEntity>().LongCount(whereExpression);

        public virtual Task<long> LongCountAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default) => DbContext.Set<TEntity>().LongCountAsync(whereExpression, cancellationToken);

        public virtual int Delete(Expression<Func<TEntity, bool>> whereExpression)
        {
            DbContext.Set<TEntity>().RemoveRange(DbContext.Set<TEntity>().Where(whereExpression));
            return DbContext.SaveChanges();
        }

        public virtual Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            DbContext.Set<TEntity>().RemoveRange(DbContext.Set<TEntity>().Where(whereExpression));
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual bool Exist(Expression<Func<TEntity, bool>> whereExpression) =>
            DbContext.Set<TEntity>().Any(whereExpression);

        public virtual Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default) =>
            DbContext.Set<TEntity>().AnyAsync(whereExpression, cancellationToken);

        public virtual TEntity Fetch(Expression<Func<TEntity, bool>> whereExpression)
        => DbContext.Set<TEntity>().AsNoTracking().FirstOrDefault(whereExpression);

        public virtual Task<TEntity> FetchAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)

        => DbContext.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(whereExpression, cancellationToken);

        public virtual TEntity Fetch<TProperty>(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false)
        {
            if (orderByExpression == null)
                return DbContext.Set<TEntity>().AsNoTracking().FirstOrDefault(whereExpression);

            return ascending
                ? DbContext.Set<TEntity>().AsNoTracking().OrderBy(orderByExpression).FirstOrDefault(whereExpression)
                : DbContext.Set<TEntity>().AsNoTracking().OrderByDescending(orderByExpression).FirstOrDefault(whereExpression)
                ;
        }

        public virtual Task<TEntity> FetchAsync<TProperty>(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false, CancellationToken cancellationToken = default)
        {
            if (orderByExpression == null)
                return DbContext.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(whereExpression, cancellationToken);

            return ascending
                ? DbContext.Set<TEntity>().AsNoTracking().OrderBy(orderByExpression).FirstOrDefaultAsync(whereExpression, cancellationToken)
                : DbContext.Set<TEntity>().AsNoTracking().OrderByDescending(orderByExpression).FirstOrDefaultAsync(whereExpression, cancellationToken)
                ;
        }

        public virtual int Insert(TEntity entity)
        {
            DbContext.Set<TEntity>().Add(entity);
            return DbContext.SaveChanges();
        }

        public virtual int Insert(IEnumerable<TEntity> entities)
        {
            DbContext.Set<TEntity>().AddRange(entities);
            return DbContext.SaveChanges();
        }

        public virtual Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            DbContext.Set<TEntity>().Add(entity);
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual Task<int> InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            DbContext.Set<TEntity>().AddRange(entities);
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual IPagedListResult<TEntity> Paged<TProperty>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false)
        {
            var total = DbContext.Set<TEntity>().AsNoTracking()
                .Count(whereExpression);
            if (total == 0)
            {
                return PagedListResult<TEntity>.Empty;
            }
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            var query = DbContext.Set<TEntity>().AsNoTracking()
                .Where(whereExpression);
            query = ascending ? query.OrderBy(orderByExpression) : query.OrderByDescending(orderByExpression);
            var data = query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToArray();
            return new PagedListResult<TEntity>()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total,
                Data = data
            };
        }

        public virtual async Task<IPagedListResult<TEntity>> PagedAsync<TProperty>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false, CancellationToken cancellationToken = default)
        {
            var total = await DbContext.Set<TEntity>().AsNoTracking()
                .CountAsync(whereExpression, cancellationToken);
            if (total == 0)
            {
                return PagedListResult<TEntity>.Empty;
            }
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            var query = DbContext.Set<TEntity>().AsNoTracking()
                .Where(whereExpression);
            query = ascending ? query.OrderBy(orderByExpression) : query.OrderByDescending(orderByExpression);
            var data = await query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync(cancellationToken);
            return new PagedListResult<TEntity>()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total,
                Data = data
            };
        }

        public virtual List<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression) => DbContext.Set<TEntity>().AsNoTracking()
                .Where(whereExpression).ToList();

        public virtual List<TEntity> Select<TProperty>(int count, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false)
        {
            var query = DbContext.Set<TEntity>().AsNoTracking().Where(whereExpression);
            if (ascending)
            {
                query = query.OrderBy(orderByExpression);
            }
            else
            {
                query = query.OrderByDescending(orderByExpression);
            }
            return query.Take(count).ToList();
        }

        public virtual Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default) => DbContext.Set<TEntity>().AsNoTracking().Where(whereExpression).ToListAsync(cancellationToken);

        public virtual Task<List<TEntity>> SelectAsync<TProperty>(int count, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false, CancellationToken cancellationToken = default)
        {
            var query = DbContext.Set<TEntity>().AsNoTracking().Where(whereExpression);
            if (ascending)
            {
                query = query.OrderBy(orderByExpression);
            }
            else
            {
                query = query.OrderByDescending(orderByExpression);
            }
            return query.Take(count).ToListAsync(cancellationToken);
        }

        public virtual int Update<TProperty>(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> propertyExpression, object value)
        {
            foreach (var entity in DbContext.Set<TEntity>().Where(whereExpression))
            {
                entity.SetPropertyValue(propertyExpression.GetMemberName(), value);
            }

            return DbContext.SaveChanges();
        }

        public virtual int Update(Expression<Func<TEntity, bool>> whereExpression, IDictionary<string, object> propertyValues)
        {
            foreach (var entity in DbContext.Set<TEntity>().Where(whereExpression))
            {
                foreach (var propertyValue in propertyValues)
                {
                    entity.SetPropertyValue(propertyValue.Key, propertyValue.Value);
                }
            }

            return DbContext.SaveChanges();
        }

        public virtual Task<int> UpdateAsync<TProperty>(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> propertyExpression, object value, CancellationToken cancellationToken = default)
        {
            foreach (var entity in DbContext.Set<TEntity>().Where(whereExpression))
            {
                entity.SetPropertyValue(propertyExpression.GetMemberName(), value);
            }

            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<int> UpdateAsync(Expression<Func<TEntity, bool>> whereExpression, IDictionary<string, object> propertyValues, CancellationToken cancellationToken = default)
        {
            foreach (var entity in DbContext.Set<TEntity>().Where(whereExpression))
            {
                foreach (var propertyValue in propertyValues)
                {
                    entity.SetPropertyValue(propertyValue.Key, propertyValue.Value);
                }
            }

            return await DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual int Update(TEntity entity, params Expression<Func<TEntity, object>>[] propertyExpressions)
        {
            DbContext.Update(entity, propertyExpressions);
            return DbContext.SaveChanges();
        }

        public virtual int UpdateWithout(TEntity entity, params Expression<Func<TEntity, object>>[] propertyExpressions)
        {
            DbContext.UpdateWithout(entity, propertyExpressions);
            return DbContext.SaveChanges();
        }

        public virtual Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyExpressions, CancellationToken cancellationToken = default)
        {
            DbContext.Update(entity, propertyExpressions);
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual Task<int> UpdateWithoutAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyExpressions,
            CancellationToken cancellationToken = default)
        {
            DbContext.UpdateWithout(entity, propertyExpressions);
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual int Update(TEntity entity, params string[] propertyNames)
        {
            DbContext.Update(entity, propertyNames);
            return DbContext.SaveChanges();
        }

        public virtual int UpdateWithout(TEntity entity, params string[] propertyNames)
        {
            DbContext.UpdateWithout(entity, propertyNames);
            return DbContext.SaveChanges();
        }

        public virtual Task<int> UpdateAsync(TEntity entity, string[] propertyNames, CancellationToken cancellationToken = default)
        {
            DbContext.Update(entity, propertyNames);
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual Task<int> UpdateWithoutAsync(TEntity entity, string[] propertyNames,
            CancellationToken cancellationToken = default)
        {
            DbContext.UpdateWithout(entity, propertyNames);
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual int Delete(TEntity entity)
        {
            if (entity == null)
            {
                return 0;
            }

            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Deleted;
            return DbContext.SaveChanges();
        }

        public virtual Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                return Task.FromResult(0);
            }

            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Deleted;
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual IQueryable<TEntity> Query(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(DbContext.Set<TEntity>());
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build();
        }

        public virtual List<TEntity> Get(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(DbContext.Set<TEntity>());
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().ToList();
        }

        public virtual List<TResult> GetResult<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(DbContext.Set<TEntity>());
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build(selector).ToList();
        }

        public virtual Task<List<TEntity>> GetAsync(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, CancellationToken cancellationToken = default)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(DbContext.Set<TEntity>());
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().ToListAsync(cancellationToken);
        }

        public virtual Task<List<TResult>> GetResultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null,
            CancellationToken cancellationToken = default)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(DbContext.Set<TEntity>());
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build(selector).ToListAsync(cancellationToken);
        }

        public virtual bool Any(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(DbContext.Set<TEntity>());
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().Any();
        }

        public virtual Task<bool> AnyAsync(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, CancellationToken cancellationToken = default)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(DbContext.Set<TEntity>());
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().AnyAsync(cancellationToken);
        }

        public virtual TEntity FirstOrDefault(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(DbContext.Set<TEntity>());
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().FirstOrDefault();
        }

        public virtual TResult FirstOrDefaultResult<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(DbContext.Set<TEntity>());
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build(selector).FirstOrDefault();
        }

        public virtual Task<TEntity> FirstOrDefaultAsync(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, CancellationToken cancellationToken = default)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(DbContext.Set<TEntity>());
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().FirstOrDefaultAsync(cancellationToken);
        }

        public virtual Task<TResult> FirstOrDefaultResultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, CancellationToken cancellationToken = default)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(DbContext.Set<TEntity>());
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build(selector).FirstOrDefaultAsync(cancellationToken);
        }

        public virtual IPagedListResult<TEntity> GetPagedList(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, int pageNumber = 1, int pageSize = 20)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(DbContext.Set<TEntity>());
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().ToPagedList(pageNumber, pageSize);
        }

        public virtual Task<IPagedListResult<TEntity>> GetPagedListAsync(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, int pageNumber = 1, int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(DbContext.Set<TEntity>());
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        }

        public virtual IPagedListResult<TResult> GetPagedListResult<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, int pageNumber = 1,
            int pageSize = 20)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(DbContext.Set<TEntity>());
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build(selector).ToPagedList(pageNumber, pageSize);
        }

        public virtual Task<IPagedListResult<TResult>> GetPagedListResultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, int pageNumber = 1,
            int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(DbContext.Set<TEntity>());
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build(selector).ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        }
    }
}
