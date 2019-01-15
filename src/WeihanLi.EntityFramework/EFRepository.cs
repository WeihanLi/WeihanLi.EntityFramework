using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeihanLi.Common.Data;
using WeihanLi.Common.Models;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework
{
    public class EFRepository<TDbContext, TEntity> : IRepository<TEntity>
        where TDbContext : DbContext
        where TEntity : class
    {
        private readonly TDbContext _dbContext;

        public EFRepository(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public virtual long Count(Expression<Func<TEntity, bool>> whereExpression) => _dbContext.Set<TEntity>().AsNoTracking().LongCount(whereExpression);

        public virtual Task<long> CountAsync(Expression<Func<TEntity, bool>> whereExpression)
        => _dbContext.Set<TEntity>().AsNoTracking().LongCountAsync(whereExpression);

        public virtual int Delete(Expression<Func<TEntity, bool>> whereExpression)
        {
            _dbContext.Set<TEntity>().RemoveRange(_dbContext.Set<TEntity>().Where(whereExpression));
            return _dbContext.SaveChanges();
        }

        public virtual Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression)
        {
            _dbContext.Set<TEntity>().RemoveRange(_dbContext.Set<TEntity>().Where(whereExpression));
            return _dbContext.SaveChangesAsync();
        }

        public virtual int Execute(string sqlStr, object param = null)
        {
            throw new NotImplementedException();
        }

        public virtual Task<int> ExecuteAsync(string sqlStr, object param = null)
        {
            throw new NotImplementedException();
        }

        public virtual TResult ExecuteScalar<TResult>(string sqlStr, object param = null)
        {
            throw new NotImplementedException();
        }

        public virtual Task<TResult> ExecuteScalarAsync<TResult>(string sqlStr, object param = null)
        {
            throw new NotImplementedException();
        }

        public virtual bool Exist(Expression<Func<TEntity, bool>> whereExpression) => _dbContext.Set<TEntity>().AsNoTracking()
                .Any(whereExpression);

        public virtual Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression) => _dbContext.Set<TEntity>().AsNoTracking()
                .AnyAsync(whereExpression);

        public virtual TEntity Fetch(Expression<Func<TEntity, bool>> whereExpression)
        => _dbContext.Set<TEntity>().AsNoTracking().FirstOrDefault(whereExpression);

        public virtual Task<TEntity> FetchAsync(Expression<Func<TEntity, bool>> whereExpression)

        => _dbContext.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(whereExpression);

        public virtual int Insert(TEntity entity)
        {
            _dbContext.Set<TEntity>().Add(entity);
            return _dbContext.SaveChanges();
        }

        public virtual int Insert(IEnumerable<TEntity> entities)
        {
            _dbContext.Set<TEntity>().AddRange(entities);
            return _dbContext.SaveChanges();
        }

        public virtual Task<int> InsertAsync(TEntity entity)
        {
            _dbContext.Set<TEntity>().Add(entity);
            return _dbContext.SaveChangesAsync();
        }

        public virtual Task<int> InsertAsync(IEnumerable<TEntity> entities)
        {
            _dbContext.Set<TEntity>().AddRange(entities);
            return _dbContext.SaveChangesAsync();
        }

        public virtual PagedListModel<TEntity> Paged<TProperty>(int pageIndex, int pageSize, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> orderByExpression, bool isAsc = false)
        {
            var total = _dbContext.Set<TEntity>().AsNoTracking()
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
            var query = _dbContext.Set<TEntity>().AsNoTracking()
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
            var total = await _dbContext.Set<TEntity>().AsNoTracking()
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
            var query = _dbContext.Set<TEntity>().AsNoTracking()
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

        public virtual IEnumerable<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression) => _dbContext.Set<TEntity>().AsNoTracking()
                .Where(whereExpression).AsEnumerable();

        public virtual Task<IEnumerable<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression)
        {
            return Task.FromResult(_dbContext.Set<TEntity>().AsNoTracking().Where(whereExpression).AsEnumerable());
        }

        public virtual int Update<TProperty>(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> propertyExpression, object value)
        {
            foreach (var entity in _dbContext.Set<TEntity>().Where(whereExpression))
            {
                entity.SetPropertyValue(propertyExpression.GetMemberName(), value);
            }

            return _dbContext.SaveChanges();
        }

        public virtual int Update(Expression<Func<TEntity, bool>> whereExpression, IDictionary<string, object> propertyValues)
        {
            foreach (var entity in _dbContext.Set<TEntity>().Where(whereExpression))
            {
                foreach (var propertyValue in propertyValues)
                {
                    entity.SetPropertyValue(propertyValue.Key, propertyValue.Value);
                }
            }

            return _dbContext.SaveChanges();
        }

        public virtual async Task<int> UpdateAsync<TProperty>(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TProperty>> propertyExpression, object value)
        {
            foreach (var entity in _dbContext.Set<TEntity>().Where(whereExpression))
            {
                entity.SetPropertyValue(propertyExpression.GetMemberName(), value);
            }

            return await _dbContext.SaveChangesAsync();
        }

        public virtual async Task<int> UpdateAsync(Expression<Func<TEntity, bool>> whereExpression, IDictionary<string, object> propertyValues)
        {
            foreach (var entity in _dbContext.Set<TEntity>().Where(whereExpression))
            {
                foreach (var propertyValue in propertyValues)
                {
                    entity.SetPropertyValue(propertyValue.Key, propertyValue.Value);
                }
            }

            return await _dbContext.SaveChangesAsync();
        }
    }
}
