using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeihanLi.Common.Models;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework
{
    public class EFRepository<TDbContext, TEntity> :
        IEFRepository<TEntity>
        where TDbContext : DbContext
        where TEntity : class
    {
        protected readonly TDbContext DbContext;

        public EFRepository(TDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public virtual long Count(Expression<Func<TEntity, bool>> whereExpression) => DbContext.Set<TEntity>().AsNoTracking().LongCount(whereExpression);

        public virtual Task<long> CountAsync(Expression<Func<TEntity, bool>> whereExpression)
        => DbContext.Set<TEntity>().AsNoTracking().LongCountAsync(whereExpression);

        public virtual int Delete(Expression<Func<TEntity, bool>> whereExpression)
        {
            DbContext.Set<TEntity>().RemoveRange(DbContext.Set<TEntity>().Where(whereExpression));
            return DbContext.SaveChanges();
        }

        public virtual Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression)
        {
            DbContext.Set<TEntity>().RemoveRange(DbContext.Set<TEntity>().Where(whereExpression));
            return DbContext.SaveChangesAsync();
        }

        public virtual bool Exist(Expression<Func<TEntity, bool>> whereExpression) => DbContext.Set<TEntity>().AsNoTracking()
                .Any(whereExpression);

        public virtual Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression) => DbContext.Set<TEntity>().AsNoTracking()
                .AnyAsync(whereExpression);

        public virtual TEntity Fetch(Expression<Func<TEntity, bool>> whereExpression)
        => DbContext.Set<TEntity>().AsNoTracking().FirstOrDefault(whereExpression);

        public virtual Task<TEntity> FetchAsync(Expression<Func<TEntity, bool>> whereExpression)

        => DbContext.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(whereExpression);

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

        public virtual Task<int> InsertAsync(TEntity entity)
        {
            DbContext.Set<TEntity>().Add(entity);
            return DbContext.SaveChangesAsync();
        }

        public virtual Task<int> InsertAsync(IEnumerable<TEntity> entities)
        {
            DbContext.Set<TEntity>().AddRange(entities);
            return DbContext.SaveChangesAsync();
        }

        public virtual PagedListModel<TEntity> Paged<TProperty>(int pageIndex, int pageSize, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool isAsc = false)
        {
            var total = DbContext.Set<TEntity>().AsNoTracking()
                .Count(whereExpression);
            if (total == 0)
            {
                return new PagedListModel<TEntity>() { PageIndex = 1, PageSize = pageSize, TotalCount = 0 };
            }
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            var query = DbContext.Set<TEntity>().AsNoTracking()
                .Where(whereExpression);
            if (isAsc)
            {
                query = query.OrderBy(orderByExpression);
            }
            else
            {
                query = query.OrderByDescending(orderByExpression);
            }
            var data = query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToArray();
            return new PagedListModel<TEntity>()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = total,
                Data = data
            };
        }

        public virtual async Task<PagedListModel<TEntity>> PagedAsync<TProperty>(int pageIndex, int pageSize, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool isAsc = false)
        {
            var total = await DbContext.Set<TEntity>().AsNoTracking()
                .CountAsync(whereExpression);
            if (total == 0)
            {
                return new PagedListModel<TEntity>() { PageIndex = 1, PageSize = pageSize, TotalCount = 0 };
            }
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 10;
            }
            var query = DbContext.Set<TEntity>().AsNoTracking()
                .Where(whereExpression);
            if (isAsc)
            {
                query = query.OrderBy(orderByExpression);
            }
            else
            {
                query = query.OrderByDescending(orderByExpression);
            }
            var data = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync();
            return new PagedListModel<TEntity>()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = total,
                Data = data
            };
        }

        public virtual List<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression) => DbContext.Set<TEntity>().AsNoTracking()
                .Where(whereExpression).ToList();

        public List<TEntity> Select<TProperty>(int count, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool isAsc = false)
        {
            var query = DbContext.Set<TEntity>().AsNoTracking().Where(whereExpression);
            if (isAsc)
            {
                query = query.OrderBy(orderByExpression);
            }
            else
            {
                query = query.OrderByDescending(orderByExpression);
            }
            return query.Take(count).ToList();
        }

        public virtual Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression) => DbContext.Set<TEntity>().AsNoTracking().Where(whereExpression).ToListAsync();

        public Task<List<TEntity>> SelectAsync<TProperty>(int count, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool isAsc = false)
        {
            var query = DbContext.Set<TEntity>().AsNoTracking().Where(whereExpression);
            if (isAsc)
            {
                query = query.OrderBy(orderByExpression);
            }
            else
            {
                query = query.OrderByDescending(orderByExpression);
            }
            return query.Take(count).ToListAsync();
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

        public int Update(TEntity entity)
        {
            DbContext.Update(entity);
            return DbContext.SaveChanges();
        }

        public int Update(TEntity entity, string[] parameters)
        {
            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Unchanged;
            foreach (var param in parameters)
            {
                entry.Property(param).IsModified = true;
            }
            return DbContext.SaveChanges();
        }

        public int Update<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Unchanged;
            entry.Property(propertyExpression).IsModified = true;
            return DbContext.SaveChanges();
        }

        public virtual Task<int> UpdateAsync<TProperty>(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> propertyExpression, object value)
        {
            foreach (var entity in DbContext.Set<TEntity>().Where(whereExpression))
            {
                entity.SetPropertyValue(propertyExpression.GetMemberName(), value);
            }

            return DbContext.SaveChangesAsync();
        }

        public virtual async Task<int> UpdateAsync(Expression<Func<TEntity, bool>> whereExpression, IDictionary<string, object> propertyValues)
        {
            foreach (var entity in DbContext.Set<TEntity>().Where(whereExpression))
            {
                foreach (var propertyValue in propertyValues)
                {
                    entity.SetPropertyValue(propertyValue.Key, propertyValue.Value);
                }
            }

            return await DbContext.SaveChangesAsync();
        }

        public Task<int> UpdateAsync(TEntity entity)
        {
            DbContext.Set<TEntity>().Update(entity);
            return DbContext.SaveChangesAsync();
        }

        public Task<int> UpdateAsync(TEntity entity, string[] parameters)
        {
            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Unchanged;
            foreach (var param in parameters)
            {
                entry.Property(param).IsModified = true;
            }
            return DbContext.SaveChangesAsync();
        }

        public Task<int> UpdateAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            var entry = DbContext.Set<TEntity>().Attach(entity);
            entry.State = EntityState.Unchanged;
            entry.Property(propertyExpression).IsModified = true;
            return DbContext.SaveChangesAsync();
        }
    }
}
