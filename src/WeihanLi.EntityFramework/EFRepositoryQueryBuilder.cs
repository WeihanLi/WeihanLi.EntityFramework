using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using WeihanLi.Common;

namespace WeihanLi.EntityFramework;

public class EFRepositoryQueryBuilder<TEntity> where TEntity : class
{
    private readonly DbSet<TEntity> _dbSet;

    public EFRepositoryQueryBuilder(DbSet<TEntity> dbSet)
    {
        _dbSet = dbSet;
    }

    private readonly List<Expression<Func<TEntity, bool>>> _whereExpression = new();

    public EFRepositoryQueryBuilder<TEntity> WithPredict(Expression<Func<TEntity, bool>> predict)
    {
        _whereExpression.Add(Guard.NotNull(predict));
        return this;
    }

    public EFRepositoryQueryBuilder<TEntity> WithPredictIf(Expression<Func<TEntity, bool>> predict, bool condition)
    {
        if (condition)
            _whereExpression.Add(Guard.NotNull(predict));
        return this;
    }

    private Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? _orderByExpression;

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
    
    private readonly HashSet<string> _queryFiltersToIgnore = new();
    
    public EFRepositoryQueryBuilder<TEntity> IgnoreQueryFilters(IReadOnlyCollection<string> queryFilters, bool ignoreQueryFilters = true)
    {
        ArgumentNullException.ThrowIfNull(queryFilters);
        if (ignoreQueryFilters)
        {
            foreach (var queryFilter in queryFilters)
            {
                _queryFiltersToIgnore.Add(queryFilter);
            }
        }
        else
        {
            foreach (var queryFilter in queryFilters)
            {
                _queryFiltersToIgnore.Remove(queryFilter);
            }
        }
        return this;
    }

    private int _count;

    public EFRepositoryQueryBuilder<TEntity> WithCount(int count)
    {
        _count = count;
        return this;
    }

    private readonly List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object?>>> _includeExpressions = new();

    public EFRepositoryQueryBuilder<TEntity> WithInclude(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object?>> include)
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
        else if (_queryFiltersToIgnore.Count > 0)
        {
            query = query.IgnoreQueryFilters(_queryFiltersToIgnore);
        }
        if (_whereExpression.Count > 0)
        {
            foreach (var expression in _whereExpression)
            {
                query = query.Where(expression);
            }
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
        ArgumentNullException.ThrowIfNull(selector);
        var query = Build();
        return query.Select(selector);
    }

    public IQueryable<TResult> Build<TResult>(Expression<Func<TEntity, int, TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var query = Build();
        return query.Select(selector);
    }
}
