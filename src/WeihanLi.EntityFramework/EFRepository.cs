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
        protected readonly TDbContext DbContext;
        protected readonly DbSet<TEntity> EntitySet;

        public EFRepository(TDbContext dbContext)
        {
            DbContext = dbContext;
            EntitySet = dbContext.Set<TEntity>();
        }

        public virtual int Count(Expression<Func<TEntity, bool>> whereExpression) => EntitySet.AsNoTracking().Count(whereExpression);

        public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        => EntitySet.AsNoTracking().CountAsync(whereExpression, cancellationToken);

        public virtual long LongCount(Expression<Func<TEntity, bool>> whereExpression) => EntitySet.AsNoTracking().LongCount(whereExpression);

        public virtual Task<long> LongCountAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default) => EntitySet.AsNoTracking().LongCountAsync(whereExpression, cancellationToken);

        public virtual int Delete(Expression<Func<TEntity, bool>> whereExpression)
        {
            EntitySet.RemoveRange(EntitySet.Where(whereExpression));
            return DbContext.SaveChanges();
        }

        public virtual Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        {
            EntitySet.RemoveRange(EntitySet.Where(whereExpression));
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual bool Exist(Expression<Func<TEntity, bool>> whereExpression) => EntitySet.AsNoTracking()
                .Any(whereExpression);

        public virtual Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default) => EntitySet.AsNoTracking()
                .AnyAsync(whereExpression, cancellationToken);

        public virtual TEntity Fetch(Expression<Func<TEntity, bool>> whereExpression)
        => EntitySet.AsNoTracking().FirstOrDefault(whereExpression);

        public virtual Task<TEntity> FetchAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)

        => EntitySet.AsNoTracking().FirstOrDefaultAsync(whereExpression, cancellationToken);

        public virtual TEntity Fetch<TProperty>(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false)
        {
            if (orderByExpression == null)
                return EntitySet.AsNoTracking().FirstOrDefault(whereExpression);

            return ascending
                ? EntitySet.AsNoTracking().OrderBy(orderByExpression).FirstOrDefault(whereExpression)
                : EntitySet.AsNoTracking().OrderByDescending(orderByExpression).FirstOrDefault(whereExpression)
                ;
        }

        public virtual Task<TEntity> FetchAsync<TProperty>(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false, CancellationToken cancellationToken = default)
        {
            if (orderByExpression == null)
                return EntitySet.AsNoTracking().FirstOrDefaultAsync(whereExpression, cancellationToken);

            return ascending
                ? EntitySet.AsNoTracking().OrderBy(orderByExpression).FirstOrDefaultAsync(whereExpression, cancellationToken)
                : EntitySet.AsNoTracking().OrderByDescending(orderByExpression).FirstOrDefaultAsync(whereExpression, cancellationToken)
                ;
        }

        public virtual int Insert(TEntity entity)
        {
            EntitySet.Add(entity);
            return DbContext.SaveChanges();
        }

        public virtual int Insert(IEnumerable<TEntity> entities)
        {
            EntitySet.AddRange(entities);
            return DbContext.SaveChanges();
        }

        public virtual Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            EntitySet.Add(entity);
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual Task<int> InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            EntitySet.AddRange(entities);
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual IPagedListModel<TEntity> Paged<TProperty>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false)
        {
            var total = EntitySet.AsNoTracking()
                .Count(whereExpression);
            if (total == 0)
            {
                return new PagedListModel<TEntity>() { PageSize = pageSize };
            }
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            var query = EntitySet.AsNoTracking()
                .Where(whereExpression);
            query = ascending ? query.OrderBy(orderByExpression) : query.OrderByDescending(orderByExpression);
            var data = query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToArray();
            return new PagedListModel<TEntity>()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total,
                Data = data
            };
        }

        public virtual async Task<IPagedListModel<TEntity>> PagedAsync<TProperty>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false, CancellationToken cancellationToken = default)
        {
            var total = await EntitySet.AsNoTracking()
                .CountAsync(whereExpression, cancellationToken);
            if (total == 0)
            {
                return new PagedListModel<TEntity>() { PageSize = pageSize };
            }
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            var query = EntitySet.AsNoTracking()
                .Where(whereExpression);
            query = ascending ? query.OrderBy(orderByExpression) : query.OrderByDescending(orderByExpression);
            var data = await query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync(cancellationToken);
            return new PagedListModel<TEntity>()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total,
                Data = data
            };
        }

        public virtual List<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression) => EntitySet.AsNoTracking()
                .Where(whereExpression).ToList();

        public virtual List<TEntity> Select<TProperty>(int count, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false)
        {
            var query = EntitySet.AsNoTracking().Where(whereExpression);
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

        public virtual Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default) => EntitySet.AsNoTracking().Where(whereExpression).ToListAsync(cancellationToken);

        public virtual Task<List<TEntity>> SelectAsync<TProperty>(int count, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false, CancellationToken cancellationToken = default)
        {
            var query = EntitySet.AsNoTracking().Where(whereExpression);
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
            foreach (var entity in EntitySet.Where(whereExpression))
            {
                entity.SetPropertyValue(propertyExpression.GetMemberName(), value);
            }

            return DbContext.SaveChanges();
        }

        public virtual int Update(Expression<Func<TEntity, bool>> whereExpression, IDictionary<string, object> propertyValues)
        {
            foreach (var entity in EntitySet.Where(whereExpression))
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
            foreach (var entity in EntitySet.Where(whereExpression))
            {
                entity.SetPropertyValue(propertyExpression.GetMemberName(), value);
            }

            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<int> UpdateAsync(Expression<Func<TEntity, bool>> whereExpression, IDictionary<string, object> propertyValues, CancellationToken cancellationToken = default)
        {
            foreach (var entity in EntitySet.Where(whereExpression))
            {
                foreach (var propertyValue in propertyValues)
                {
                    entity.SetPropertyValue(propertyValue.Key, propertyValue.Value);
                }
            }

            return await DbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyExpressions, CancellationToken cancellationToken = default)
        {
            if (propertyExpressions == null || propertyExpressions.Length == 0)
            {
                EntitySet.Update(entity);
            }
            else
            {
                var entry = DbContext.Set<TEntity>().Attach(entity);
                entry.State = EntityState.Unchanged;
                foreach (var expression in propertyExpressions)
                {
                    entry.Property(expression).IsModified = true;
                }
            }
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<int> UpdateWithoutAsync(TEntity entity, Expression<Func<TEntity, object>>[] propertyExpressions,
            CancellationToken cancellationToken = default)
        {
            if (propertyExpressions == null || propertyExpressions.Length == 0)
            {
                EntitySet.Update(entity);
            }
            else
            {
                var entry = DbContext.Set<TEntity>().Attach(entity);
                entry.State = EntityState.Modified;
                foreach (var expression in propertyExpressions)
                {
                    entry.Property(expression).IsModified = false;
                }
            }
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public int Update(TEntity entity, params Expression<Func<TEntity, object>>[] propertyExpressions)
        {
            if (propertyExpressions == null || propertyExpressions.Length == 0)
            {
                EntitySet.Update(entity);
            }
            else
            {
                var entry = DbContext.Set<TEntity>().Attach(entity);
                entry.State = EntityState.Unchanged;
                foreach (var expression in propertyExpressions)
                {
                    entry.Property(expression).IsModified = true;
                }
            }
            return DbContext.SaveChanges();
        }

        public int UpdateWithout(TEntity entity, params Expression<Func<TEntity, object>>[] propertyExpressions)
        {
            if (propertyExpressions == null || propertyExpressions.Length == 0)
            {
                EntitySet.Update(entity);
            }
            else
            {
                var entry = DbContext.Set<TEntity>().Attach(entity);
                entry.State = EntityState.Modified;
                foreach (var expression in propertyExpressions)
                {
                    entry.Property(expression).IsModified = false;
                }
            }
            return DbContext.SaveChanges();
        }

        public virtual TEntity Find(params object[] keyValues)
        {
            return EntitySet.Find(keyValues);
        }

        public virtual Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken)
        {
            return EntitySet.FindAsync(keyValues, cancellationToken);
        }

        public virtual List<TEntity> Get(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(EntitySet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().ToList();
        }

        public virtual List<TResult> Get<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(EntitySet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build(selector).ToList();
        }

        public virtual Task<List<TEntity>> GetAsync(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, CancellationToken cancellationToken = default)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(EntitySet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().ToListAsync(cancellationToken);
        }

        public virtual Task<List<TResult>> GetAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null,
            CancellationToken cancellationToken = default)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(EntitySet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build(selector).ToListAsync(cancellationToken);
        }

        public virtual TEntity FirstOrDefault(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(EntitySet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().FirstOrDefault();
        }

        public virtual TResult FirstOrDefault<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(EntitySet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build(selector).FirstOrDefault();
        }

        public virtual Task<TEntity> FirstOrDefaultAsync(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, CancellationToken cancellationToken = default)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(EntitySet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().FirstOrDefaultAsync(cancellationToken);
        }

        public virtual Task<TResult> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, CancellationToken cancellationToken = default)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(EntitySet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build(selector).FirstOrDefaultAsync(cancellationToken);
        }

        public virtual IPagedListModel<TEntity> GetPagedList(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, int pageNumber = 1, int pageSize = 20)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(EntitySet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().ToPagedList(pageNumber, pageSize);
        }

        public virtual Task<IPagedListModel<TEntity>> GetPagedListAsync(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, int pageNumber = 1, int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(EntitySet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        }

        public virtual IPagedListModel<TResult> GetPagedList<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, int pageNumber = 1,
            int pageSize = 20)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(EntitySet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build(selector).ToPagedList(pageNumber, pageSize);
        }

        public virtual Task<IPagedListModel<TResult>> GetPagedListAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, int pageNumber = 1,
            int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(EntitySet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build(selector).ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        }
    }
}
