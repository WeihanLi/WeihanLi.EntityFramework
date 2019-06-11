using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using WeihanLi.Extensions;

namespace WeihanLi.EntityFramework
{
    public class EFRepoQueryBuilder<TEntity> where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;

        public EFRepoQueryBuilder(DbSet<TEntity> dbSet)
        {
            _dbSet = dbSet;
        }

        private Expression<Func<TEntity, bool>> _whereExpression = t => true;

        public EFRepoQueryBuilder<TEntity> WithPredict(Expression<Func<TEntity, bool>> predict)
        {
            _whereExpression = _whereExpression.And(predict);
            return this;
        }

        private Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> _orderByExpression;

        public EFRepoQueryBuilder<TEntity> WithOrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderByExpression)
        {
            _orderByExpression = orderByExpression;
            return this;
        }

        private bool _disableTracking = true;

        public EFRepoQueryBuilder<TEntity> WithNoTracking(bool noTracking = true)
        {
            _disableTracking = noTracking;
            return this;
        }

        private bool _ignoreQueryFilters;

        public EFRepoQueryBuilder<TEntity> IgnoreQueryFilters(bool ignoreQueryFilters = true)
        {
            _ignoreQueryFilters = ignoreQueryFilters;
            return this;
        }

        private int _count;

        public EFRepoQueryBuilder<TEntity> WithCount(int count)
        {
            _count = count;
            return this;
        }

        private readonly List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> _includeExpressions = new List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>>();

        public EFRepoQueryBuilder<TEntity> WithInclude(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include)
        {
            _includeExpressions.Add(include);
            return this;
        }

        public IQueryable<TEntity> BuildQuery()
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
            query = query.Where(_whereExpression);
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

        public IQueryable<TResult> BuildQuery<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            if (null == selector)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var query = BuildQuery();
            return query.Select(selector);
        }
    }
}
