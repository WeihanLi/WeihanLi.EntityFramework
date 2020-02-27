using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework
{
    public class EFRepositoryQueryBuilder<TEntity> where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;

        public EFRepositoryQueryBuilder(DbSet<TEntity> dbSet)
        {
            _dbSet = dbSet;
        }

        private Expression<Func<TEntity, bool>> _whereExpression;

        public EFRepositoryQueryBuilder<TEntity> WithPredict(Expression<Func<TEntity, bool>> predict)
        {
            if (null == _whereExpression)
            {
                _whereExpression = predict;
            }
            else
            {
                _whereExpression = _whereExpression.And(predict);
            }
            return this;
        }

        private Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> _orderByExpression;

        public EFRepositoryQueryBuilder<TEntity> WithOrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderByExpression)
        {
            _orderByExpression = orderByExpression;
            return this;
        }

        private bool _disableTracking = true;

        public EFRepositoryQueryBuilder<TEntity> WithNoTracking(bool noTracking = true)
        {
            _disableTracking = noTracking;
            return this;
        }

        private bool _ignoreQueryFilters;

        public EFRepositoryQueryBuilder<TEntity> IgnoreQueryFilters(bool ignoreQueryFilters = true)
        {
            _ignoreQueryFilters = ignoreQueryFilters;
            return this;
        }

        private int _count;

        public EFRepositoryQueryBuilder<TEntity> WithCount(int count)
        {
            _count = count;
            return this;
        }

        private readonly List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> _includeExpressions = new List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>>();

        public EFRepositoryQueryBuilder<TEntity> WithInclude(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include)
        {
            _includeExpressions.Add(include);
            return this;
        }

        public IQueryable<TEntity> Build()
        {
            IQueryable<TEntity> query = _dbSet;
            if (_disableTracking)
            {
                query = _dbSet.AsNoTracking();
            }
            if (_ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }
            if (_whereExpression != null)
            {
                query = query.Where(_whereExpression);
            }
            if (_orderByExpression != null)
            {
                query = _orderByExpression(query);
            }
            if (_count > 0)
            {
                query = query.Take(_count);
            }
            foreach (var include in _includeExpressions)
            {
                query = include(query);
            }
            return query;
        }

        public IQueryable<TResult> Build<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            if (null == selector)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var query = Build();
            return query.Select(selector);
        }

        public IQueryable<TResult> Build<TResult>(Expression<Func<TEntity, int, TResult>> selector)
        {
            if (null == selector)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var query = Build();
            return query.Select(selector);
        }
    }
}
