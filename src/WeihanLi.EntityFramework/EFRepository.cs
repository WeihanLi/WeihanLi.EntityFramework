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
        protected readonly DbSet<TEntity> _dbSet;

        public EFRepository(TDbContext dbContext)
        {
            DbContext = dbContext;
            _dbSet = dbContext.Set<TEntity>();
        }

        public virtual int Count(Expression<Func<TEntity, bool>> whereExpression) => DbContext.Set<TEntity>().AsNoTracking().Count(whereExpression);

        public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default)
        => DbContext.Set<TEntity>().AsNoTracking().CountAsync(whereExpression, cancellationToken);

        public virtual long LongCount(Expression<Func<TEntity, bool>> whereExpression) => DbContext.Set<TEntity>().AsNoTracking().LongCount(whereExpression);

        public virtual Task<long> LongCountAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default) => DbContext.Set<TEntity>().AsNoTracking().LongCountAsync(whereExpression, cancellationToken);

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

        public virtual bool Exist(Expression<Func<TEntity, bool>> whereExpression) => DbContext.Set<TEntity>().AsNoTracking()
                .Any(whereExpression);

        public virtual Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default) => DbContext.Set<TEntity>().AsNoTracking()
                .AnyAsync(whereExpression, cancellationToken);

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

        public virtual IPagedListModel<TEntity> Paged<TProperty>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool ascending = false)
        {
            var total = DbContext.Set<TEntity>().AsNoTracking()
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
            var query = DbContext.Set<TEntity>().AsNoTracking()
                .Where(whereExpression);
            if (ascending)
            {
                query = query.OrderBy(orderByExpression);
            }
            else
            {
                query = query.OrderByDescending(orderByExpression);
            }
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
            var total = await DbContext.Set<TEntity>().AsNoTracking()
                .CountAsync(whereExpression);
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
            var query = DbContext.Set<TEntity>().AsNoTracking()
                .Where(whereExpression);
            if (ascending)
            {
                query = query.OrderBy(orderByExpression);
            }
            else
            {
                query = query.OrderByDescending(orderByExpression);
            }
            var data = await query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync();
            return new PagedListModel<TEntity>()
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

        public virtual int Update(TEntity entity, params string[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                DbContext.Set<TEntity>().Update(entity);
            }
            else
            {
                var entry = DbContext.Set<TEntity>().Attach(entity);
                entry.State = EntityState.Unchanged;
                foreach (var param in parameters)
                {
                    entry.Property(param).IsModified = true;
                }
            }
            return DbContext.SaveChanges();
        }

        public virtual int Update<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Unchanged;
            entry.Property(propertyExpression).IsModified = true;
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

        public virtual Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default, params string[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                DbContext.Set<TEntity>().Update(entity);
            }
            else
            {
                var entry = DbContext.Set<TEntity>().Attach(entity);
                entry.State = EntityState.Unchanged;
                foreach (var param in parameters)
                {
                    entry.Property(param).IsModified = true;
                }
            }
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual Task<int> UpdateAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression, CancellationToken cancellationToken = default)
        {
            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Unchanged;
            entry.Property(propertyExpression).IsModified = true;
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual int Update<TProperty1, TProperty2>(TEntity entity, Expression<Func<TEntity, TProperty1>> propertyExpression1, Expression<Func<TEntity, TProperty2>> propertyExpression2)
        {
            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Unchanged;
            entry.Property(propertyExpression1).IsModified = true;
            entry.Property(propertyExpression2).IsModified = true;
            return DbContext.SaveChanges();
        }

        public virtual int Update<TProperty1, TProperty2, TProperty3>(TEntity entity, Expression<Func<TEntity, TProperty1>> propertyExpression1, Expression<Func<TEntity, TProperty2>> propertyExpression2, Expression<Func<TEntity, TProperty3>> propertyExpression3)
        {
            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Unchanged;
            entry.Property(propertyExpression1).IsModified = true;
            entry.Property(propertyExpression2).IsModified = true;
            entry.Property(propertyExpression3).IsModified = true;
            return DbContext.SaveChanges();
        }

        public virtual int Update<TProperty1, TProperty2, TProperty3, TProperty4>(TEntity entity, Expression<Func<TEntity, TProperty1>> propertyExpression1, Expression<Func<TEntity, TProperty2>> propertyExpression2, Expression<Func<TEntity, TProperty3>> propertyExpression3, Expression<Func<TEntity, TProperty4>> propertyExpression4)
        {
            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Unchanged;
            entry.Property(propertyExpression1).IsModified = true;
            entry.Property(propertyExpression2).IsModified = true;
            entry.Property(propertyExpression3).IsModified = true;
            entry.Property(propertyExpression4).IsModified = true;
            return DbContext.SaveChanges();
        }

        public virtual int Update<TProperty1, TProperty2, TProperty3, TProperty4, TProperty5>(TEntity entity,
            Expression<Func<TEntity, TProperty1>> propertyExpression1, Expression<Func<TEntity, TProperty2>> propertyExpression2, Expression<Func<TEntity, TProperty3>> propertyExpression3, Expression<Func<TEntity, TProperty4>> propertyExpression4, Expression<Func<TEntity, TProperty5>> propertyExpression5)
        {
            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Unchanged;
            entry.Property(propertyExpression1).IsModified = true;
            entry.Property(propertyExpression2).IsModified = true;
            entry.Property(propertyExpression3).IsModified = true;
            entry.Property(propertyExpression4).IsModified = true;
            entry.Property(propertyExpression5).IsModified = true;
            return DbContext.SaveChanges();
        }

        public virtual int Update<TProperty1, TProperty2, TProperty3, TProperty4, TProperty5, TProperty6>(TEntity entity,
            Expression<Func<TEntity, TProperty1>> propertyExpression1,
            Expression<Func<TEntity, TProperty2>> propertyExpression2,
            Expression<Func<TEntity, TProperty3>> propertyExpression3,
            Expression<Func<TEntity, TProperty4>> propertyExpression4,
            Expression<Func<TEntity, TProperty5>> propertyExpression5,
            Expression<Func<TEntity, TProperty6>> propertyExpression6)
        {
            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Unchanged;
            entry.Property(propertyExpression1).IsModified = true;
            entry.Property(propertyExpression2).IsModified = true;
            entry.Property(propertyExpression3).IsModified = true;
            entry.Property(propertyExpression4).IsModified = true;
            entry.Property(propertyExpression5).IsModified = true;
            entry.Property(propertyExpression6).IsModified = true;
            return DbContext.SaveChanges();
        }

        public virtual Task<int> UpdateAsync<TProperty1, TProperty2>(TEntity entity, Expression<Func<TEntity, TProperty1>> propertyExpression1, Expression<Func<TEntity, TProperty2>> propertyExpression2, CancellationToken cancellationToken = default)
        {
            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Unchanged;
            entry.Property(propertyExpression1).IsModified = true;
            entry.Property(propertyExpression2).IsModified = true;
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual Task<int> UpdateAsync<TProperty1, TProperty2, TProperty3>(TEntity entity, Expression<Func<TEntity, TProperty1>> propertyExpression1,
            Expression<Func<TEntity, TProperty2>> propertyExpression2,
            Expression<Func<TEntity, TProperty3>> propertyExpression3, CancellationToken cancellationToken = default)
        {
            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Unchanged;
            entry.Property(propertyExpression1).IsModified = true;
            entry.Property(propertyExpression2).IsModified = true;
            entry.Property(propertyExpression3).IsModified = true;
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual Task<int> UpdateAsync<TProperty1, TProperty2, TProperty3, TProperty4>(TEntity entity,
            Expression<Func<TEntity, TProperty1>> propertyExpression1,
            Expression<Func<TEntity, TProperty2>> propertyExpression2,
            Expression<Func<TEntity, TProperty3>> propertyExpression3,
            Expression<Func<TEntity, TProperty4>> propertyExpression4, CancellationToken cancellationToken = default)
        {
            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Unchanged;
            entry.Property(propertyExpression1).IsModified = true;
            entry.Property(propertyExpression2).IsModified = true;
            entry.Property(propertyExpression3).IsModified = true;
            entry.Property(propertyExpression4).IsModified = true;
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual Task<int> UpdateAsync<TProperty1, TProperty2, TProperty3, TProperty4, TProperty5>(TEntity entity, Expression<Func<TEntity, TProperty1>> propertyExpression1, Expression<Func<TEntity, TProperty2>> propertyExpression2, Expression<Func<TEntity, TProperty3>> propertyExpression3, Expression<Func<TEntity, TProperty4>> propertyExpression4, Expression<Func<TEntity, TProperty5>> propertyExpression5, CancellationToken cancellationToken = default)
        {
            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Unchanged;
            entry.Property(propertyExpression1).IsModified = true;
            entry.Property(propertyExpression2).IsModified = true;
            entry.Property(propertyExpression3).IsModified = true;
            entry.Property(propertyExpression4).IsModified = true;
            entry.Property(propertyExpression5).IsModified = true;
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual Task<int> UpdateAsync<TProperty1, TProperty2, TProperty3, TProperty4, TProperty5, TProperty6>(TEntity entity, Expression<Func<TEntity, TProperty1>> propertyExpression1, Expression<Func<TEntity, TProperty2>> propertyExpression2, Expression<Func<TEntity, TProperty3>> propertyExpression3, Expression<Func<TEntity, TProperty4>> propertyExpression4, Expression<Func<TEntity, TProperty5>> propertyExpression5, Expression<Func<TEntity, TProperty6>> propertyExpression6, CancellationToken cancellationToken = default)
        {
            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Unchanged;
            entry.Property(propertyExpression1).IsModified = true;
            entry.Property(propertyExpression2).IsModified = true;
            entry.Property(propertyExpression3).IsModified = true;
            entry.Property(propertyExpression4).IsModified = true;
            entry.Property(propertyExpression5).IsModified = true;
            entry.Property(propertyExpression6).IsModified = true;
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public List<TEntity> Get(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(_dbSet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().ToList();
        }

        public List<TResult> Get<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(_dbSet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build(selector).ToList();
        }

        public Task<List<TEntity>> GetAsync(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, CancellationToken cancellationToken = default)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(_dbSet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().ToListAsync(cancellationToken);
        }

        public Task<List<TResult>> GetAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null,
            CancellationToken cancellationToken = default)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(_dbSet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build(selector).ToListAsync(cancellationToken);
        }

        public IPagedListModel<TEntity> Paged(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, int pageNumber = 1, int pageSize = 20)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(_dbSet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().ToPagedList(pageNumber, pageSize);
        }

        public Task<IPagedListModel<TEntity>> PagedAsync(Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, int pageNumber = 1, int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(_dbSet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build().ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        }

        public IPagedListModel<TResult> Paged<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, int pageNumber = 1,
            int pageSize = 20) where TResult : class
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(_dbSet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build(selector).ToPagedList(pageNumber, pageSize);
        }

        public Task<IPagedListModel<TResult>> PagedAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Action<EFRepositoryQueryBuilder<TEntity>> queryBuilderAction = null, int pageNumber = 1,
            int pageSize = 20, CancellationToken cancellationToken = default) where TResult : class
        {
            var queryBuilder = new EFRepositoryQueryBuilder<TEntity>(_dbSet);
            queryBuilderAction?.Invoke(queryBuilder);

            return queryBuilder.Build(selector).ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        }

        public TEntity Find(params object[] keyValues)
        {
            return DbContext.Set<TEntity>().Find(keyValues);
        }

        public Task<TEntity> FindAsync(params object[] keyValues)
        {
            return DbContext.Set<TEntity>().FindAsync(keyValues);
        }

        public Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken)
        {
            return DbContext.Set<TEntity>().FindAsync(keyValues, cancellationToken);
        }

        //public IPagedListModel<TEntity> Paged(Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, int pageNumber = 1, int pageSize = 20, bool disableTracking = true, bool ignoreQueryFilter = false)
        //{
        //    IQueryable<TEntity> query = DbContext.Set<TEntity>();
        //    if (disableTracking)
        //    {
        //        query = query.AsNoTracking();
        //    }

        //    if (include != null)
        //    {
        //        query = include(query);
        //    }

        //    if (predicate != null)
        //    {
        //        query = query.Where(predicate);
        //    }

        //    if (orderBy != null)
        //    {
        //        return orderBy(query).ToPagedList(pageNumber, pageSize);
        //    }
        //    else
        //    {
        //        return query.ToPagedList(pageNumber, pageSize);
        //    }
        //}

        //public Task<IPagedListModel<TEntity>> PagedAsync(Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, int pageNumber = 1, int pageSize = 20, bool disableTracking = true, bool ignoreQueryFilter = false, CancellationToken cancellationToken = default)
        //{
        //    IQueryable<TEntity> query = DbContext.Set<TEntity>();
        //    if (disableTracking)
        //    {
        //        query = query.AsNoTracking();
        //    }

        //    if (include != null)
        //    {
        //        query = include(query);
        //    }

        //    if (predicate != null)
        //    {
        //        query = query.Where(predicate);
        //    }

        //    if (orderBy != null)
        //    {
        //        return orderBy(query).ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        //    }
        //    else
        //    {
        //        return query.ToPagedListAsync(pageNumber, pageSize, cancellationToken);
        //    }
        //}

        //public IPagedListModel<TResult> Paged<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, int pageNumber = 1, int pageSize = 20, bool disableTracking = true, bool ignoreQueryFilter = false) where TResult : class
        //{
        //    IQueryable<TEntity> query = _dbSet;
        //    if (disableTracking)
        //    {
        //        query = query.AsNoTracking();
        //    }

        //    if (include != null)
        //    {
        //        query = include(query);
        //    }

        //    if (predicate != null)
        //    {
        //        query = query.Where(predicate);
        //    }

        //    if (orderBy != null)
        //    {
        //        return orderBy(query).Select(selector).ToPagedList(pageNumber, pageSize);
        //    }
        //    else
        //    {
        //        return query.Select(selector).ToPagedList(pageNumber, pageSize);
        //    }
        //}

        //public Task<IPagedListModel<TResult>> PagedAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, int pageNumber = 1, int pageSize = 20, bool disableTracking = true, bool ignoreQueryFilter = false, CancellationToken cancellationToken = default) where TResult : class
        //{
        //    IQueryable<TEntity> query = _dbSet;
        //    if (disableTracking)
        //    {
        //        query = query.AsNoTracking();
        //    }

        //    if (include != null)
        //    {
        //        query = include(query);
        //    }

        //    if (predicate != null)
        //    {
        //        query = query.Where(predicate);
        //    }

        //    if (orderBy != null)
        //    {
        //        return orderBy(query).Select(selector).ToPagedListAsync(pageNumber, pageSize);
        //    }
        //    else
        //    {
        //        return query.Select(selector).ToPagedListAsync(pageNumber, pageSize);
        //    }
        //}

        //public List<TEntity> Get(Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false)
        //{
        //    IQueryable<TEntity> query = _dbSet;
        //    if (disableTracking)
        //    {
        //        query = query.AsNoTracking();
        //    }

        //    if (ignoreQueryFilter)
        //    {
        //        query = query.IgnoreQueryFilters();
        //    }

        //    if (include != null)
        //    {
        //        query = include(query);
        //    }

        //    if (predicate != null)
        //    {
        //        query = query.Where(predicate);
        //    }

        //    if (orderBy != null)
        //    {
        //        return orderBy(query).ToList();
        //    }
        //    else
        //    {
        //        return query.ToList();
        //    }
        //}

        //public List<TResult> Get<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false)
        //{
        //    IQueryable<TEntity> query = _dbSet;
        //    if (disableTracking)
        //    {
        //        query = query.AsNoTracking();
        //    }
        //    if (ignoreQueryFilter)
        //    {
        //        query = query.IgnoreQueryFilters();
        //    }
        //    if (include != null)
        //    {
        //        query = include(query);
        //    }
        //    if (predicate != null)
        //    {
        //        query = query.Where(predicate);
        //    }
        //    if (orderBy != null)
        //    {
        //        return orderBy(query).Select(selector).ToList();
        //    }
        //    else
        //    {
        //        return query.Select(selector).ToList();
        //    }
        //}

        //public Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false, CancellationToken cancellationToken = default)
        //{
        //    IQueryable<TEntity> query = _dbSet;
        //    if (disableTracking)
        //    {
        //        query = query.AsNoTracking();
        //    }
        //    if (ignoreQueryFilter)
        //    {
        //        query = query.IgnoreQueryFilters();
        //    }
        //    if (include != null)
        //    {
        //        query = include(query);
        //    }

        //    if (predicate != null)
        //    {
        //        query = query.Where(predicate);
        //    }

        //    if (orderBy != null)
        //    {
        //        return orderBy(query).ToListAsync(cancellationToken);
        //    }
        //    else
        //    {
        //        return query.ToListAsync(cancellationToken);
        //    }
        //}

        //public Task<List<TResult>> GetAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, bool disableTracking = true, bool ignoreQueryFilter = false, CancellationToken cancellationToken = default)
        //{
        //    IQueryable<TEntity> query = _dbSet;
        //    if (disableTracking)
        //    {
        //        query = query.AsNoTracking();
        //    }
        //    if (ignoreQueryFilter)
        //    {
        //        query = query.IgnoreQueryFilters();
        //    }
        //    if (include != null)
        //    {
        //        query = include(query);
        //    }

        //    if (predicate != null)
        //    {
        //        query = query.Where(predicate);
        //    }

        //    if (orderBy != null)
        //    {
        //        return orderBy(query).Select(selector).ToListAsync(cancellationToken);
        //    }
        //    else
        //    {
        //        return query.Select(selector).ToListAsync(cancellationToken);
        //    }
        //}
    }
}
